using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G28_SP24_WebSellingPaint.FunctionCode;
using SEP490_G28_SP24_WebSellingPaint.Models;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SEP490_G28_SP24_WebSellingPaint.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly WebSellingPaintingContext _context;

        public AdminController(ILogger<AdminController> logger, WebSellingPaintingContext context)
        {
            _logger = logger;
            _context = context;
        }

        #region Admin Promotion
        private void loadDataForPromotionAdmin(int startDate, int endDate, decimal minReduce, decimal maxReduce,
            int status, decimal fromMinOrderPrice, decimal toMinOrderPrice)
        {
            var promotionList = _context.Vouchers
                .Where(u => (u.StartDate >= startDate && u.StartDate < endDate)
                && (u.Percentage >= minReduce && u.Percentage < maxReduce)
                && (u.MinOrderValue >= fromMinOrderPrice && u.MinOrderValue < toMinOrderPrice))
                .Select(p => new
                {
                    VoucherId = p.VoucherId,
                    VoucherName = p.VoucherName,
                    StartDate = DateTime.ParseExact(p.StartDate.ToString(), "yyyyMMdd", null).ToString("dd/MM/yyyy"),
                    EndDate = DateTime.ParseExact(p.EndDate.ToString(), "yyyyMMdd", null).ToString("dd/MM/yyyy"),
                    StatusId = p.StatusId,
                    StatusName = p.Status.StatusName
                })
                .ToList();
            //filter by status
            if (status > 1)
            {
                promotionList = promotionList.Where(p => p.StatusId == status).ToList();
            }

            //get status list
            var statusList = _context.Statuses.Include(t => t.Type)
                .Where(t => t.Type.TypeName == "Promotion")
                .Select(s => new
                {
                    ID = s.StatusId,
                    Name = s.StatusName
                }).ToList();

            ViewBag.promotionList = promotionList;
            ViewBag.statusList = statusList;
        }

        public IActionResult PromotionAdmin()
        {
            ViewBag.ActiveTitle = "promotion";
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return View("Error");
            }
            decimal min = 0;
            decimal max = 999999999;
            loadDataForPromotionAdmin((int)min, (int)max, min, max, (int)min, min, max);
            return View();
        }

        #endregion

        #region Admin Promotion Filter
        [HttpPost]
        public IActionResult PromotionAdmin(IFormCollection form)
        {
            Dict dict = new Dict
            {
                {"activeFrom", form["activeFrom"].ToString()},
                {"activeTo", form["activeTo"].ToString()},
                {"reduceFrom", form["reduceFrom"].ToString()},
                {"reduceTo", form["reduceTo"].ToString()},
                {"status", form["status"].ToString()},
                {"minOrderPrice", form["minOrderPrice"].ToString()},
                {"maxOrderPrice", form["maxOrderPrice"].ToString()}
            };

            return RedirectToAction("PostPromotionAdmin", dict);
        }

        public IActionResult PostPromotionAdmin(Dict form)
        {
            int startDate = form["activeFrom"].ToString() == "" ? 0 :
                int.Parse(form["activeFrom"].ToString().Replace("-", ""));
            int endDate = form["activeTo"].ToString() == "" ? 999999999 :
                int.Parse(form["activeTo"].ToString().Replace("-", ""));
            decimal minReduce = form["reduceFrom"].ToString() == "" ? 0 :
                decimal.Parse(form["reduceFrom"].ToString());
            decimal maxReduce = form["reduceTo"].ToString() == "" ? 999999999 :
                decimal.Parse(form["reduceTo"].ToString());
            int status = int.Parse(form["status"].ToString());
            decimal minOrderPrice = form["minOrderPrice"].ToString() == "" ? 0 :
                decimal.Parse(form["minOrderPrice"].ToString());
            decimal maxOrderPrice = form["maxOrderPrice"].ToString() == "" ? 999999999 :
                decimal.Parse(form["maxOrderPrice"].ToString());

            loadDataForPromotionAdmin(startDate, endDate, minReduce, maxReduce,
                status, minOrderPrice, maxOrderPrice);

            ViewBag.startDate = startDate == 0 ? "" : form["activeFrom"].ToString();
            ViewBag.endDate = endDate == 999999999 ? "" : form["activeTo"].ToString();
            ViewBag.minReduce = minReduce == 0 ? "" : form["reduceFrom"].ToString();
            ViewBag.maxReduce = maxReduce == 999999999 ? "" : form["reduceTo"].ToString();
            ViewBag.status = status;
            ViewBag.minOrderPrice = minOrderPrice == 0 ? "" : form["minOrderPrice"].ToString();
            ViewBag.maxOrderPrice = maxOrderPrice == 999999999 ? "" : form["maxOrderPrice"].ToString();

            return View("PromotionAdmin");
        }
        #endregion

        #region Admin Promotion Detail
        public IActionResult PromotionDetailAdmin(int id)
        {
            ViewBag.ActiveTitle = "promotion";
            ViewBag.ActiveTitle = "promotion";
            int UserID = (int)HttpContext.Session.GetInt32("UserID");
            var promotionDetail = _context.Vouchers
                .Where(v => v.VoucherId == id)
                .Select(v => new
                {
                    ID = v.VoucherId,
                    VoucherCode = v.VoucherCode,
                    Percentage = v.Percentage,
                    MinOrderValue = v.MinOrderValue,
                    Logo = _context.Images
                    .Where(i => i.ObjectId == id && i.Type.TypeName == "Promotion")
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault(),
                    VoucherName = v.VoucherName,
                    StartDate = DateTime.ParseExact(v.StartDate.ToString(), "yyyyMMdd", null).ToString("dd/MM/yyyy"),
                    EndDate = DateTime.ParseExact(v.EndDate.ToString(), "yyyyMMdd", null).ToString("dd/MM/yyyy"),
                    StatusName = v.Status.StatusName
                })
                .SingleOrDefault();
            if (promotionDetail == null | HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return View("Error");
            }

            ViewBag.promotion = promotionDetail;
            return View();
        }

        #endregion

        #region Admin Profile Page
        public IActionResult ProfileAdmin()
        {
            ViewBag.ActiveTitle = "profile";
            if (HttpContext.Session.GetString("UserRole")!="Admin")
            {
                return View("Error");
            }
            int userID = (int)HttpContext.Session.GetInt32("UserID");
            var user = _context.Users
                .Where(u => u.UserId == userID)
                .Select(u => new
                {
                    ID = u.UserId,
                    Name = u.UserName,
                    ShopName = u.ShopName,
                    Background = u.ArtistBackground,
                    Avatar = _context.Images
                    .Where(i => i.ObjectId == u.UserId && i.Type.TypeName == "User")
                    .Select(i => i.ImageUrl).FirstOrDefault(),
                    Phone = u.PhoneNumber,
                    Email = u.Account.Email,
                    Gender = u.Gender
                }).FirstOrDefault();
            ViewBag.user = user;
            return View();
        }
        #endregion

        #region Admin Profile Update
        [HttpPost]
        public IActionResult AdminUpdateProfile(IFormCollection form)
        {
            ViewBag.ActiveTitle = "profile";
            String imageData = form["imageData"].ToString();
            String name = form["username"].ToString().Trim();
            String phone = form["phonenumber"].ToString().Trim();
            int userid = (int)HttpContext.Session.GetInt32("UserID");
            String genderString = form["gender"].ToString();
            bool gender = genderString == "1" ? true : false;


            var checkDuplicatePhone = _context.Users.SingleOrDefault(u => u.PhoneNumber == phone && u.UserId != userid);
            if (checkDuplicatePhone != null)
            {
                TempData["ErrorMessage"] = "Enter phone number is existed! please enter a new one";
            }
            else
            {
                //save user avatar if there is
                if (imageData.Length > 200)
                {
                    String fileName = "";
                    String newAvatarUrl = "";
                    String nameSaveToDB = "";
                    do
                    {
                        newAvatarUrl = CommonMethod.GenerateRandomString(15);
                        fileName = "wwwroot/Image/User/" + newAvatarUrl + ".jpg";
                        nameSaveToDB = "/Image/User/" + newAvatarUrl + ".jpg";
                    } while (_context.Images.Where(i => i.ImageUrl == nameSaveToDB) == null);
                    CommonMethod.SaveBase64Image(imageData, fileName);
                    var avatar = _context.Images
                                         .Where(i => i.Type.TypeName == "User"
                                         && i.ObjectId == userid).FirstOrDefault();
                    avatar.ImageUrl = nameSaveToDB;
                }

                //save name, phone
                var user = _context.Users.Where(u => u.UserId == userid).FirstOrDefault();
                user.PhoneNumber = phone;
                user.UserName = name;
                user.Gender = gender;
                _context.SaveChanges();
            }

            return RedirectToAction("ProfileAdmin", "Admin");
        }

        #endregion

        #region Admin UserManagerment
        public IActionResult UserManagement()
        {
            ViewBag.ActiveTitle = "user";
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return View("Error");
            }

            var userManagerList = (from account in _context.Accounts
                                   join user in _context.Users on account.AccountId equals user.AccountId
                                   join role in _context.Roles on account.RoleId equals role.RoleId
                                   where role.RoleName != "Admin" && role.RoleName != "Manager"
                                   select new
                                   {
                                       AccountID = account.AccountId,
                                       AccountName = account.Email,
                                       UserName = user.UserName,
                                       RoleName = role.RoleName,
                                       StatusId = account.StatusId,
                                       statusName = account.Status.StatusName
                                   }
            ).ToList();

            var userRoleList = _context.Roles.Where(r => r.RoleName != "Admin" && r.RoleName != "Manager").ToList();
            var accountStatusList = _context.Statuses.Where(r => r.StatusName == "Enable" || r.StatusName == "Disable").ToList();

            ViewBag.userManagerList = userManagerList;
            ViewBag.userRoleList = userRoleList;
            ViewBag.accountStatusList = accountStatusList;
            ViewBag.emailAlert = TempData["NameErrorMessage"];
            ViewBag.nameAlert = TempData["EmailErrorMessage"];
            return View();
        }


        #endregion

        #region Admin UserManagerment Filter
        [HttpPost]
        public IActionResult UserManagement(IFormCollection form)
        {
            ViewBag.ActiveTitle = "user";
            int accountDropdown = int.Parse(form["accountDropdown"]);
            int roleDropdown = int.Parse(form["roleDropdown"]);

            var userRoleList = _context.Roles.Where(r => r.RoleName != "Admin" && r.RoleName != "Manager").ToList();
            var accountStatusList = _context.Statuses.Where(r => r.StatusName == "Enable" || r.StatusName == "Disable").ToList();

            ViewBag.userRoleList = userRoleList;
            ViewBag.accountStatusList = accountStatusList;

            ViewBag.roleDropdown = roleDropdown;
            ViewBag.accountDropdown = accountDropdown;


            if (accountDropdown != 0 && roleDropdown != 0)
            {
                var userManagerList = (from account in _context.Accounts
                                       join user in _context.Users on account.AccountId equals user.AccountId
                                       join role in _context.Roles on account.RoleId equals role.RoleId
                                       where role.RoleId == roleDropdown && account.StatusId == accountDropdown
                                       select new
                                       {
                                           AccountID = account.AccountId,
                                           AccountName = account.Email,
                                           UserName = user.UserName,
                                           RoleName = role.RoleName,
                                           StatusId = account.StatusId,
                                           statusName = account.Status.StatusName
                                       }
              ).ToList();
                ViewBag.userManagerList = userManagerList;
            }
            else if (accountDropdown == 0 && roleDropdown != 0)
            {
                var userManagerList = (from account in _context.Accounts
                                       join user in _context.Users on account.AccountId equals user.AccountId
                                       join role in _context.Roles on account.RoleId equals role.RoleId
                                       where role.RoleId == roleDropdown && (account.Status.StatusName == "Enable"
                                       || account.Status.StatusName == "Disable")
                                       select new
                                       {
                                           AccountID = account.AccountId,
                                           AccountName = account.Email,
                                           UserName = user.UserName,
                                           RoleName = role.RoleName,
                                           StatusId = account.StatusId,
                                           statusName = account.Status.StatusName
                                       }
              ).ToList();
                ViewBag.userManagerList = userManagerList;
            }
            else if (accountDropdown == 0 && roleDropdown == 0)
            {
                var userManagerList = (from account in _context.Accounts
                                       join user in _context.Users on account.AccountId equals user.AccountId
                                       join role in _context.Roles on account.RoleId equals role.RoleId
                                       where (role.RoleName != "Admin" && role.RoleName != "Manager") && (account.Status.StatusName == "Enable"
                                       || account.Status.StatusName == "Disable")
                                       select new
                                       {
                                           AccountID = account.AccountId,
                                           AccountName = account.Email,
                                           UserName = user.UserName,
                                           RoleName = role.RoleName,
                                           StatusId = account.StatusId,
                                           statusName = account.Status.StatusName
                                       }
              ).ToList();
                ViewBag.userManagerList = userManagerList;
            }
            else
            {
                var userManagerList = (from account in _context.Accounts
                                       join user in _context.Users on account.AccountId equals user.AccountId
                                       join role in _context.Roles on account.RoleId equals role.RoleId
                                       where (role.RoleName != "Admin" && role.RoleName != "Manager") && account.StatusId == accountDropdown
                                       select new
                                       {
                                           AccountID = account.AccountId,
                                           AccountName = account.Email,
                                           UserName = user.UserName,
                                           RoleName = role.RoleName,
                                           StatusId = account.StatusId,
                                           statusName = account.Status.StatusName
                                       }
              ).ToList();
                ViewBag.userManagerList = userManagerList;
            }
            return View();
        }
        #endregion

        #region Admin UserManagerment Status
        [HttpPost]
        public IActionResult UserManagementAction(IFormCollection form)
        {
            ViewBag.ActiveTitle = "user";

            string accountId = form["adminAction"].ToString();

            int convertAccountId = int.Parse(accountId);

            var updateUserAccountStatus = _context.Accounts.SingleOrDefault(u => u.AccountId == convertAccountId);

            var getEnableAccountStatus = _context.Statuses.SingleOrDefault(s => s.StatusName == "Enable");

            var getDisableAccountStatus = _context.Statuses.SingleOrDefault(s => s.StatusName == "Disable");

            if (updateUserAccountStatus != null)
            {
                if (updateUserAccountStatus.Status.StatusName == "Enable")
                {
                    updateUserAccountStatus.StatusId = getDisableAccountStatus.StatusId;
                    _context.SaveChanges();

                }
                else
                {
                    updateUserAccountStatus.StatusId = getEnableAccountStatus.StatusId;
                    _context.SaveChanges();
                }

            }

            return RedirectToAction("UserManagement", "Admin");
        }
        #endregion

        #region Admin Create Supervisor Account

        [HttpPost]
        public IActionResult CreateSupervisorAccount(IFormCollection form)
        {
            ViewBag.ActiveTitle = "user";
            string supervisorAccount = form["supervisorEmail"].ToString().Trim();
            string supervisorName = form["supervisorName"];
            string supervisorgender = form["gender"];
            string commonPassword = "laptrinhvien";
            bool gender = true;
            if (supervisorgender.Equals("female"))
            {
                gender = false;
            }

            if (!CommonMethod.IsValidEmail(supervisorAccount.Trim()))
            {
                TempData["EmailErrorMessage"] = "Enter supervisor email is invalid";
            }
            else if (_context.Accounts.Any(a => a.Email == supervisorAccount))
            {
                TempData["EmailErrorMessage"] = "Enter supervisor email email has been registered. Please enter new one";
            }
            else
            {
                var newAccount = new Account()
                {
                    Email = supervisorAccount,
                    RoleId = CommonMethod.getRoleID("Supervisor"),
                    Password = CommonMethod.EncodePassword(commonPassword),
                    StatusId = CommonMethod.getStatusID("Account", "Enable")
                };

                _context.Accounts.Add(newAccount);
                _context.SaveChanges();

                var newUser = new User
                {
                    UserName = supervisorName,
                    PhoneNumber = "",
                    AccountId = newAccount.AccountId,
                    Gender = gender,
                    ShopName = "",
                    ArtistBackground = ""

                };

                _context.Users.Add(newUser);
                _context.SaveChanges();
            }

            return RedirectToAction("UserManagement", "Admin");

        }
        #endregion

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}