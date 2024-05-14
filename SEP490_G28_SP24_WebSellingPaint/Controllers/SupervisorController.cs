using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G28_SP24_WebSellingPaint.FeatureCode;
using SEP490_G28_SP24_WebSellingPaint.FunctionCode;
using SEP490_G28_SP24_WebSellingPaint.Models;
using System.Globalization;

namespace SEP490_G28_SP24_WebSellingPaint.Controllers
{
    public class SupervisorController : Controller
    {
        private WebSellingPaintingContext _context;

        public SupervisorController(WebSellingPaintingContext context)
        {
            _context = context;
        }

        #region Painting Management for supervisor
        private void loadPaintingManagementData(String name, int status)
        {
            //get status data
            var statuses = _context.Statuses.Where(s => s.Type.TypeName == "Painting")
                .Select(s => new
                {
                    ID = s.StatusId,
                    Name = s.StatusName
                }).ToList();

            //get painting data
            var paintings = _context.Paintings.Include(t => t.Status)
                .Where(p => p.Name.ToLower().Contains(name.ToLower()))
                .Select(p => new
                {
                    ID = p.PaintingId
                    ,
                    Name = p.Name
                    ,
                    Description = p.Description
                    ,
                    StatusID = p.StatusId
                    ,
                    StatusName = p.Status.StatusName
                }).ToList();
            //filter by status
            if (status > 0)
            {
                paintings = paintings.Where(p => p.StatusID == status).ToList();
            }

            ViewBag.painting = paintings;
            ViewBag.status = statuses;
        }

        [HttpGet]
        public IActionResult PaintingManagement()
        {
            if (HttpContext.Session.GetString("UserRole") != "Supervisor")
            {
                return View("Error");
            }
            ViewBag.ActiveTitle = "painting";
            ViewBag.name = "";
            ViewBag.statusID = 0;
            loadPaintingManagementData("", 0);
            return View();
        }


        [HttpPost]
        public IActionResult PaintingManagement(IFormCollection form)
        {
            ViewBag.ActiveTitle = "painting";
            ViewBag.name = form["searchBox"].ToString();
            ViewBag.statusID = int.Parse(form["status"].ToString());
            loadPaintingManagementData(form["searchBox"].ToString(), int.Parse(form["status"].ToString()));

            return View();
        }
        #endregion

        #region Painting Detail Supervisor
        public IActionResult PaintingDetailSupervisor(int id)
        {
            ViewBag.ActiveTitle = "painting";
            var painting = _context.Paintings
                .Where(p => p.PaintingId == id)
                .Select(p => new
                {
                    ID = p.PaintingId
                    ,
                    Name = p.Name
                    ,
                    Price = p.Price
                    ,
                    Height = p.Height
                    ,
                    Width = p.Width
                    ,
                    Date = DateTime.ParseExact(p.PublishDate.ToString(), "yyyyMMdd",
                    CultureInfo.InvariantCulture).ToString("yyyy-MM-dd")
                    ,
                    Quantity = p.Quantity
                    ,
                    StatusName = p.Status.StatusName
                    ,
                    Description = p.Description
                    ,
                    Discount = p.Discount.Percentage
                }).FirstOrDefault();
            if (painting == null | HttpContext.Session.GetString("UserRole") != "Supervisor")
            {
                return View("Error");
            }
            else
            {
                ViewBag.painting = painting;
                var Images = _context.Images.Where(i => i.ObjectId == painting.ID && i.Type.TypeName == "Painting")
                    .Select(i => new
                    {
                        ID = i.ImageId
                        ,
                        Url = i.ImageUrl
                    }).ToList();
                ViewBag.image = Images;
            }
            return View();
        }


        [HttpPost]
        public IActionResult CensorPainting(IFormCollection form)
        {
            int id = int.Parse(form["id"].ToString());
            var painting = _context.Paintings.Where(p => p.PaintingId == id).FirstOrDefault();
            if (form["action"].ToString() == "enable")
            {
                painting.StatusId = CommonMethod.getStatusID("Painting", "Active");

            }
            else
            {
                painting.StatusId = CommonMethod.getStatusID("Painting", "Inactive");
            }
            _context.SaveChanges();
            return Redirect("../PaintingDetailSupervisor?id=" + id);
        }
        #endregion

        #region Post Management Supervisor
        public IActionResult PostManagementSupervisor()
        {
            ViewBag.ActiveTitle = "post";
            return View();
        }
        #endregion

        #region Post Detail Supervisor
        public IActionResult PostDetailSupervisor()
        {
            ViewBag.ActiveTitle = "post";
            return View();
        }

        #endregion

        #region Profile Supervisor
        public IActionResult SupervisorProfile()
        {
            ViewBag.ActiveTitle = "profile";
            if (HttpContext.Session.GetString("UserRole")!="Supervisor")
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
                    Avatar = _context.Images
                    .Where(i => i.ObjectId == u.UserId && i.Type.TypeName == "User")
                    .Select(i => i.ImageUrl).FirstOrDefault(),
                    Phone = u.PhoneNumber,
                    Email = u.Account.Email,
                    Gender = u.Gender
                }).FirstOrDefault();
            ViewBag.user = user;
            ViewBag.phoneDuplicateAlert = TempData["ErrorMessage"];
            return View();
        }
        #endregion

        #region Update Profile Supervisor

        [HttpPost]
        public IActionResult SupervisorUpdateProfile(IFormCollection form)
        {
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

            return RedirectToAction("SupervisorProfile", "Supervisor");
        }

        #endregion

    }
}
