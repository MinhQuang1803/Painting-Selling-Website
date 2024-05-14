using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G28_SP24_WebSellingPaint.Models;
using System.Text.RegularExpressions;
using System.Text;
using System;
using SEP490_G28_SP24_WebSellingPaint.FeatureCode;
using SEP490_G28_SP24_WebSellingPaint.FunctionCode.EmailSender;
using SEP490_G28_SP24_WebSellingPaint.FunctionCode;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;

namespace SEP490_G28_SP24_WebSellingPaint.Controllers
{
    public class LoginController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly WebSellingPaintingContext _context;

        public LoginController(IEmailSender emailSender, WebSellingPaintingContext context)
        {
            this._emailSender = emailSender;
            _context = context;
        }

        #region Login
        public IActionResult Login()
        {
            //check if cookies exist
            if (Request.Cookies.ContainsKey("Email") && Request.Cookies.ContainsKey("Password"))
            {
                //store user's infor to login next time
                var Email = Request.Cookies["Email"];
                var Password = Request.Cookies["Password"];
                ViewBag.email = Email;
                ViewBag.password = CommonMethod.DecodePassword(Password);
                ViewBag.rememberme = "on";
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(IFormCollection form)
        {
            String email = form["email"];
            String password = form["password"];
            Console.WriteLine(form["rememberme"].ToString());
            var upgradeAccountCode = HttpContext.Session.GetString("userCode");
            var getHistoryTransaction = _context.TransactionHistories.SingleOrDefault(h => h.Content == upgradeAccountCode);
            using (WebSellingPaintingContext DBContext = new WebSellingPaintingContext())
            {
                var account = DBContext.Accounts
                     .Include(a => a.Status).Include(a => a.Role)
                     .FirstOrDefault(a => a.Email == email && a.Password == CommonMethod.EncodePassword(password));
                if (account == null)
                {
                    ViewBag.email = email;
                    ViewBag.alert = "Invalid email or password. Please check again";
                }
                else if (account.Status.StatusName == "Disable")
                {
                    ViewBag.email = email;
                    ViewBag.alert = "Your account has been disable. If you have any problem, " +
                        "please contact our admin.";
                }
                else
                {
                    //create cookies if user check remember me
                    if (form["rememberme"].ToString() == "on")
                    {
                        var cookiesOption = new CookieOptions { Expires = DateTime.UtcNow.AddHours(1) };
                        Response.Cookies.Append("Email", email, cookiesOption);
                        Response.Cookies.Append("Password", CommonMethod.EncodePassword(password), cookiesOption);
                    }
                    else
                    {
                        Response.Cookies.Delete("Email");
                        Response.Cookies.Delete("Password");
                    }

                    var currentUser = _context.Users.FirstOrDefault(a => a.AccountId == account.AccountId);
                    //set session for the page
                    var session = HttpContext.Session;
                    session.SetString("Email", account.Email);
                    session.SetString("UserRole", account.Role.RoleName);
                    session.SetInt32("UserID", currentUser.UserId);
                    session.SetInt32("AccountID", account.AccountId);

                    //many-many relationship. Get the cart of the userid
                    var cartList = _context.Users.Include("Paintings").Where(c =>
                    c.UserId == currentUser.UserId).Select(c => c.Paintings).FirstOrDefault();
                    int[] cartArray = cartList.Select(c => c.PaintingId).ToArray();
                    HttpContext.Session.SetComplexData("cart", cartArray);

                    //move to screens base on role
                    if (account.Role.RoleName == "Customer")
                    {
                        return Redirect("Home");
                    }
                    else if (account.Role.RoleName == "Admin")
                    {
                        return Redirect("PromotionAdmin");
                    }
                    else if (account.Role.RoleName == "Supervisor")
                    {
                        return Redirect("PaintingManagement");
                    }
                    else if (account.Role.RoleName == "Manager")
                    {
                        return Redirect("CooperatorManagement");
                    }
                    else if (account.Role.RoleName == "Artist")
                    {
                        var payAccountStatus = _context.PayAccounts.SingleOrDefault(p => p.AccountId == account.AccountId);

                        if (payAccountStatus != null)
                        {

                            DateTime todayDate = DateTime.Today;
                            var currentdate = todayDate.ToString("yyyy/MM/dd");

                            int convertCurrentdate = currentdate.ToString() == "" ? 0
                                : int.Parse(currentdate.ToString().Replace("/", "").Replace("-", ""));

                            if (payAccountStatus.ExpiredAccountDate > convertCurrentdate && payAccountStatus.StatusId == CommonMethod.getStatusID("Account", "Paid And Active"))
                            {
                                return Redirect("StatisticArtist");
                            }
                            else
                            {
                                var extendAccountCode = "WSPU" + CommonMethod.GenerateRandomString(10).ToUpper();
                                payAccountStatus.ActiveCode = extendAccountCode;
                                payAccountStatus.StatusId = CommonMethod.getStatusID("Account", "Expired Artist Account");
                                _context.SaveChanges();
                                ViewBag.success = "not";
                                return View();
                            }
                        }
                    }
                    else return Redirect("Home");
                }
                ViewBag.rememberme = form["rememberme"].ToString();
                return View();
            }
        }
        #endregion

        #region Sign out
        public IActionResult Signout()
        {
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("UserRole");
            HttpContext.Session.Remove("UserID");
            HttpContext.Session.Remove("AccountID");
            return RedirectToAction("Login", "Login");
        }
        #endregion

        #region Standard Account Signup
        [HttpGet]
        public IActionResult SignUp()
        {

            return View();
        }

        [HttpPost]
        public IActionResult SignUp(String UserName, String UserEmail, String UserPassword, String UserConfirmPassword, String gender,
            String userCity, String userDistrict, String userWards, String term, String UserPhone, String userAddressDetail)
        {
            bool checkFlag = true;
            UserName = UserName == null ? "" : UserName;
            UserEmail = UserEmail == null ? "" : UserEmail;
            userCity = userCity == null ? "" : userCity;
            userDistrict = userDistrict == null ? "" : userDistrict;
            userWards = userWards == null ? "" : userWards;
            UserPassword = UserPassword == null ? "" : UserPassword;
            UserConfirmPassword = UserConfirmPassword == null ? "" : UserConfirmPassword;
            UserPhone = UserPhone == null ? "" : UserPhone;
            userAddressDetail = userAddressDetail == null ? "" : userAddressDetail;
            ViewBag.SignupName = UserName;
            ViewBag.SignupEmail = UserEmail;
            ViewBag.SignupPhone = UserPhone;

            var checkDupPhone = _context.Users.SingleOrDefault(p => p.PhoneNumber == UserPhone);

            if (UserName.Trim().Length == 0)
            {
                checkFlag = false;
                ViewBag.namealert = "Please let us know your name";
            }
            else if (!CommonMethod.IsValidEmail(UserEmail.Trim()))
            {
                checkFlag = false;
                ViewBag.emailalert = "Your email is invalid";
            }
            else if (_context.Accounts.Any(a => a.Email == UserEmail))
            {
                checkFlag = false;
                ViewBag.emailalert = "Your email has been registered. Please login.";
            }
            else if (!CommonMethod.IsStrongPassword(UserPassword.Trim()))
            {
                checkFlag = false;
                ViewBag.passwordalert = "Your password need to have atleast 8 characters and 1 uppercase letter";
            }
            else if (UserConfirmPassword.Trim() != UserPassword.Trim())
            {
                checkFlag = false;
                ViewBag.passwordalert = "Your confirm password doesn't match with password.";
            }
            else if (String.IsNullOrEmpty(userDistrict.Trim())
                    | String.IsNullOrEmpty(userWards.Trim())
                    | String.IsNullOrEmpty(userCity.Trim())
                    | String.IsNullOrEmpty(userAddressDetail.Trim()))
            {
                checkFlag = false;
                ViewBag.password = UserPassword;
                ViewBag.addressalert = "Please let us know more about your place";
            }
            else if (term != "on")
            {
                checkFlag = false;
                ViewBag.password = UserPassword;
                ViewBag.termalert = "Please check our Term of services.";
            }
            else if (checkDupPhone != null)
            {
                checkFlag = false;
                ViewBag.phonealert = "Your phone number has been registered. Please enter new one.";
            }
            else if (!CommonMethod.IsVaildPhoneNumber(UserPhone.Trim()))
            {
                checkFlag = false;
                ViewBag.phonealert = "Your phone number is invaild";
            }
            else if (UserPhone.Trim().Length == 0)
            {
                checkFlag = false;
                ViewBag.phonealert = "Please let us know your phone number";
            }


            if (checkFlag == true)
            {
                bool userGender = true;

                if (gender.Equals("female"))
                {
                    userGender = false;
                }

                var newAccount = new Account()
                {
                    Email = UserEmail,
                    RoleId = CommonMethod.getRoleID("Customer"),
                    Password = CommonMethod.EncodePassword(UserPassword),
                    StatusId = CommonMethod.getStatusID("Account", "Enable")
                };

                _context.Accounts.Add(newAccount);
                _context.SaveChanges();
                int accountIdgen = newAccount.AccountId;

                var newUser = new User()
                {
                    UserName = UserName,
                    Gender = userGender,
                    AccountId = accountIdgen,
                    PhoneNumber = UserPhone,
                    ShopName = "",
                    ArtistBackground = "",
                };
                _context.Users.Add(newUser);
                _context.SaveChanges();
                int userIgen = newUser.UserId;

                var newAddress = new Address()
                {
                    City = userCity,
                    District = userDistrict,
                    Street = userWards,
                    Status = CommonMethod.getStatusID("Address", "Default"),
                    ObjectId = userIgen,
                    TypeId = CommonMethod.getTypeID("User"),
                    Detail = userAddressDetail
                };
                _context.Addresses.Add(newAddress);

                //add a default avatar for user
                var newAvatar = new Image()
                {
                    ImageUrl = "/Image/User/avatar.jpg",
                    ObjectId = userIgen,
                    TypeId = CommonMethod.getTypeID("User")
                };
                _context.Images.Add(newAvatar);

                _context.SaveChanges();

                ViewBag.SignupName = "";
                ViewBag.SignupEmail = "";
                ViewBag.success = "Sign up successfully";
            }
            return View();
        }
        #endregion

        #region Artist Account Signup

        [HttpGet]
        public IActionResult SignUpAsArtist()
        {

            return View();
        }

        [HttpPost]
        public IActionResult SignUpAsArtist(String UserName, String UserEmail, String UserPassword, String UserConfirmPassword, String gender,
           String userCity, String userDistrict, String userWards, String term, String shopName, String artistBackground, String UserPhone,
           String userAddressDetail)
        {
            UserName = UserName == null ? "" : UserName;
            UserEmail = UserEmail == null ? "" : UserEmail;
            userCity = userCity == null ? "" : userCity;
            userDistrict = userDistrict == null ? "" : userDistrict;
            userWards = userWards == null ? "" : userWards;
            userAddressDetail = userAddressDetail == null ? "" : userAddressDetail;
            UserPassword = UserPassword == null ? "" : UserPassword;
            UserConfirmPassword = UserConfirmPassword == null ? "" : UserConfirmPassword;
            shopName = shopName == null ? "" : shopName;
            artistBackground = artistBackground == null ? "" : artistBackground;
            ViewBag.SignupName = UserName;
            ViewBag.SignupEmail = UserEmail;
            ViewBag.artistBackground = artistBackground;
            ViewBag.shopName = shopName;
            ViewBag.SignupPhone = UserPhone;

            var checkDupPhone = _context.Users.SingleOrDefault(p => p.PhoneNumber == UserPhone);

            if (UserName.Trim().Length == 0)
            {
                ViewBag.namealert = "Please let us know your name";
            }
            else if (!CommonMethod.IsValidEmail(UserEmail.Trim()))
            {
                ViewBag.emailalert = "Your email is invalid";
            }
            else if (_context.Accounts.Any(a => a.Email == UserEmail))
            {
                ViewBag.emailalert = "Your email has been registered. Please login.";
            }
            else if (!CommonMethod.IsStrongPassword(UserPassword.Trim()))
            {
                ViewBag.passwordalert = "Your password need to have atleast 8 characters and 1 uppercase letter";
            }
            else if (UserConfirmPassword.Trim() != UserPassword.Trim())
            {
                ViewBag.passwordalert = "Your confirm password doesn't match with password.";
            }
            else if (String.IsNullOrEmpty(userDistrict.Trim())
                    | String.IsNullOrEmpty(userWards.Trim())
                    | String.IsNullOrEmpty(userCity.Trim())
                    | String.IsNullOrEmpty(userAddressDetail.Trim()))
            {
                ViewBag.password = UserPassword;
                ViewBag.addressalert = "Please let us know more about your place";
            }
            else if (term != "on")
            {
                ViewBag.password = UserPassword;
                ViewBag.termalert = "Please check our Term of services.";
            }
            else if (_context.Users.Any(u => u.ShopName == shopName))
            {
                ViewBag.shopNameError = "Your shop name has been registered. please enter new one";
            }
            else if (shopName.Trim().Length == 0)
            {
                ViewBag.shopNameError = "Please let us know your shop name";
            }
            else if (checkDupPhone != null)
            {
                ViewBag.phonealert = "Your phone number has been registered. Please enter new one.";
            }
            else if (!CommonMethod.IsVaildPhoneNumber(UserPhone.Trim()))
            {
                ViewBag.phonealert = "Your phone number is invaild";
            }
            else if (UserPhone.Trim().Length == 0)
            {
                ViewBag.phonealert = "Please let us know your phone number";
            }
            else
            {
                int userGender = 1;

                if (gender.Equals("female"))
                {
                    userGender = 0;
                }
                else
                {
                    userGender = 1;
                }

                if (artistBackground == null)
                {
                    artistBackground = "";
                }

                var session = HttpContext.Session;
                session.SetString("userEmail", UserEmail);
                session.SetString("Password", CommonMethod.EncodePassword(UserPassword));
                session.SetString("userCity", userCity);
                session.SetString("userDistrict", userDistrict);
                session.SetString("userWards", userWards);
                session.SetString("ArtistBackground", artistBackground);
                session.SetString("userAddressDetail", userAddressDetail);
                session.SetString("UserName", UserName);
                session.SetString("UserPhone", UserPhone);
                session.SetString("ShopName", shopName);
                session.SetInt32("userGender", userGender);

                var activeAccountCode = "WSPU" + CommonMethod.GenerateRandomString(10).ToUpper();
                session.SetString("activeCode", activeAccountCode);
                return RedirectToAction("ActiveAccountPayment", "Artist");
            }

            return View();

        }
        #endregion

        #region Forgot Password
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(IFormCollection form)
        {
            String email = form["email"];
            using (WebSellingPaintingContext DBContext = new WebSellingPaintingContext())
            {
                var account = DBContext.Accounts.FirstOrDefault(x => x.Email == email);
                if (!CommonMethod.IsValidEmail(email))
                {
                    ViewBag.alert = "Invalid email. Please check again";
                }
                else if (account == null)
                {
                    ViewBag.alert = "Email doesn't exist. Please sign up first";
                }
                else
                {
                    //change to new password
                    String newPassword = CommonMethod.GeneratePassword(9);
                    account.Password = CommonMethod.EncodePassword(newPassword);
                    DBContext.SaveChanges();

                    //send new password to user
                    String subject = "YOUR NEW PASSWORD";
                    String body = $"Hi,<br/>We have received a request to change password from you. " +
                        $"<br/>Here is your new password: " + newPassword;
                    await _emailSender.SendEmailAsync(email, subject, body);
                    ViewBag.noti = "Success, please check your email to re-claim your password";
                }

            }
            ViewBag.email = email;
            Console.WriteLine(email);
            return View();
        }
        #endregion

        #region Change Password
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(IFormCollection form)
        {
            String oldPass = form["old"].ToString().Trim();
            String newPass = form["new"].ToString().Trim();
            String confirmPass = form["confirm"].ToString().Trim();
            String email = HttpContext.Session.GetString("Email");
            if (email == null)
            {
                return RedirectToAction("Login", "Login");
            }

            //validate user's data
            var account = _context.Accounts
                .Where(a => a.Email == email).FirstOrDefault();
            if (CommonMethod.EncodePassword(oldPass) != account.Password)
            {
                ViewBag.alert = "Your old password is not correct";
            }
            else if (confirmPass != newPass)
            {
                ViewBag.alert = "Your confirm password is not match with new password.";
            }
            else if (!CommonMethod.IsStrongPassword(newPass))
            {
                ViewBag.alert = "Your password not strong enough. Must more than 8 chars and have at least 1 uppercase";
            }
            //after done validate then change user's password
            else
            {
                account.Password = CommonMethod.EncodePassword(newPass);
                _context.SaveChanges();
                ViewBag.success = "Change password successfully";
                return View();
            }
            ViewBag.oldPass = oldPass;
            ViewBag.newPass = newPass;
            ViewBag.confirmPass = confirmPass;
            return View();
        }
        #endregion

    }
}
