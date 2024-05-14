using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_G28_SP24_WebSellingPaint.FeatureCode;
using SEP490_G28_SP24_WebSellingPaint.FunctionCode;
using SEP490_G28_SP24_WebSellingPaint.Models;

namespace SEP490_G28_SP24_WebSellingPaint.Controllers
{
    public class ManagerController : Controller
    {
        private WebSellingPaintingContext _context;

        public ManagerController(WebSellingPaintingContext context)
        {
            _context = context;
        }

        #region Cooperator Management
        private void loadDataCoopManage(String name, int status)
        {
            var coopList = _context.ShippingUnits
                .Where(u => u.Name.ToLower().Contains(name.ToLower()))
                .Select(c => new
                {
                    ID = c.ShippingUnitId,
                    Name = c.Name,
                    Status = c.StatusId,
                    Web = c.Website,
                    StatusName = c.Status.StatusName
                }).ToList();
            if (status > 0)
            {
                coopList = coopList.Where(c => c.Status == status).ToList();
            }

            var statusList = _context.Statuses.Where(s => s.Type.TypeName == "Cooperator")
                .Select(s => new
                {
                    ID = s.StatusId,
                    Name = s.StatusName
                }).ToList();

            ViewBag.statusList = statusList;
            ViewBag.coopList = coopList;
            ViewBag.Name = name;
            ViewBag.Status = status;
            ViewBag.minDate = DateTime.Now.Year + "-" + DateTime.Now.Month.ToString("D2");
        }

        public IActionResult CooperatorManagement()
        {
            ViewBag.ActiveTitle = "coop";
            if (HttpContext.Session.GetString("UserRole") != "Manager")
            {
                return View("Error");
            }
            loadDataCoopManage("", 0);
            return View();
        }

        [HttpPost]
        public IActionResult CooperatorManagement(IFormCollection form)
        {
            ViewBag.ActiveTitle = "coop";
            String name = form["name"].ToString();
            int status = int.Parse(form["status"].ToString());
            loadDataCoopManage(name, status);
            return View();
        }
        #endregion

        #region Coop Detail

        private void loadDataForCoopDetail(int id, int year)
        {
            ViewBag.ActiveTitle = "coop";
            var coop = _context.ShippingUnits.Include(t => t.ShippingPrices).ThenInclude(t => t.Type)
                .Where(c => c.ShippingUnitId == id)
                .Select(c => new
                {
                    ID = c.ShippingUnitId,
                    Name = c.Name,
                    Status = c.StatusId,
                    StatusName = c.Status.StatusName,
                    Web = c.Website,
                    Logo = _context.Images.Where(i => i.ObjectId == c.ShippingUnitId
                    && i.Type.TypeName == "Cooperator").Select(i => i.ImageUrl).FirstOrDefault(),
                    Phone = c.PhoneNumber,
                    Email = c.Email,
                    Prices = c.ShippingPrices
                })
                .FirstOrDefault();

            //get price data
            var startWorkingCoopDate = coop.Prices
            .OrderBy(sp => sp.StartDate)
            .Select(sp => sp.StartDate)
            .FirstOrDefault();

            var endWorkingCoopDate = coop.StatusName == "Active" ? int.Parse(DateTime.Now.Year + "" + DateTime.Now.Month.ToString("D2"))
                : coop.Prices
            .OrderByDescending(sp => sp.StartDate)
            .Select(sp => sp.StartDate)
            .FirstOrDefault();
            int? minMonth = 1;
            int? maxMonth = 12;

            if (year == startWorkingCoopDate / 100)
            {
                minMonth = startWorkingCoopDate % 100;
            }
            if (year == endWorkingCoopDate / 100)
            {
                maxMonth = endWorkingCoopDate % 100;
            }

            var dictInNor = CommonMethod.getPriceList(id, year, "Incity-normal");
            var dictInHea = CommonMethod.getPriceList(id, year, "Incity-heavy");
            var dictInNorFra = CommonMethod.getPriceList(id, year, "Incity-normal-fragile");
            var dictInHeaFra = CommonMethod.getPriceList(id, year, "Incity-heavy-fragile");
            var dictOutNor = CommonMethod.getPriceList(id, year, "Outcity-normal");
            var dictOutHea = CommonMethod.getPriceList(id, year, "Outcity-heavy");
            var dictOutNorFra = CommonMethod.getPriceList(id, year, "Outcity-normal-fragile");
            var dictOutHeaFra = CommonMethod.getPriceList(id, year, "Outcity-heavy-fragile");
            var dictOutPerKM = CommonMethod.getPriceList(id, year, "Outcity-perKM");

            ViewBag.dictInNor = dictInNor;
            ViewBag.dictInNorFirst = dictInNor.FirstOrDefault().Key;
            ViewBag.dictInHea = dictInHea;
            ViewBag.dictInHeaFirst = dictInHea.FirstOrDefault().Key;
            ViewBag.dictInNorFra = dictInNorFra;
            ViewBag.dictInNorFraFirst = dictInNorFra.FirstOrDefault().Key;
            ViewBag.dictInHeaFra = dictInHeaFra;
            ViewBag.dictInHeaFraFirst = dictInHeaFra.FirstOrDefault().Key;
            ViewBag.dictOutNor = dictOutNor;
            ViewBag.dictOutNorFirst = dictOutNor.FirstOrDefault().Key;
            ViewBag.dictOutHea = dictOutHea;
            ViewBag.dictOutHeaFirst = dictOutHea.FirstOrDefault().Key;
            ViewBag.dictOutNorFra = dictOutNorFra;
            ViewBag.dictOutNorFraFirst = dictOutNorFra.FirstOrDefault().Key;
            ViewBag.dictOutHeaFra = dictOutHeaFra;
            ViewBag.dictOutHeaFraFirst = dictOutHeaFra.FirstOrDefault().Key;
            ViewBag.dictOutPerKM = dictOutPerKM;
            ViewBag.dictOutPerKMFirst = dictOutPerKM.FirstOrDefault().Key;

            ViewBag.selectedYear = year;
            ViewBag.minMonth = minMonth;
            ViewBag.maxMonth = maxMonth;
            ViewBag.year = endWorkingCoopDate / 100;
            ViewBag.startWorkingCoopDate = startWorkingCoopDate;
            ViewBag.endWorkingCoopDate = endWorkingCoopDate;

            //set data for update coop
            ViewBag.nextMonth = DateTime.Now.AddMonths(1).Year+"-"+DateTime.Now.AddMonths(1).Month.ToString("D2");
            ViewBag.inNor = CommonMethod.getCurrentCoopPrice(id, "Incity-normal");
            ViewBag.inHea = CommonMethod.getCurrentCoopPrice(id, "Incity-heavy");
            ViewBag.inNorFra = CommonMethod.getCurrentCoopPrice(id, "Incity-normal-fragile");
            ViewBag.inHeaFra = CommonMethod.getCurrentCoopPrice(id, "Incity-heavy-fragile");
            ViewBag.outNor = CommonMethod.getCurrentCoopPrice(id, "Outcity-normal");
            ViewBag.outHea = CommonMethod.getCurrentCoopPrice(id, "Outcity-heavy");
            ViewBag.outNorFra = CommonMethod.getCurrentCoopPrice(id, "Outcity-normal-fragile");
            ViewBag.outHeaFra = CommonMethod.getCurrentCoopPrice(id, "Outcity-heavy-fragile");
            ViewBag.outPerKM = CommonMethod.getCurrentCoopPrice(id, "Outcity-perKM");

            ViewBag.coop = coop;
        }

        [HttpGet]
        public IActionResult CooperatorDetails(int id)
        {
            if (_context.ShippingUnits.Where(s => s.ShippingUnitId == id).FirstOrDefault() == null
                | HttpContext.Session.GetString("UserRole") != "Manager")
            {
                return View("Error");
            }
            loadDataForCoopDetail(id, DateTime.Now.Year);
            return View();
        }

        [HttpPost]
        public IActionResult CooperatorDetails(IFormCollection form)
        {
            int year = int.Parse(form["year"].ToString());
            int id = int.Parse(form["id"].ToString());
            loadDataForCoopDetail(id, year);
            ViewBag.selectedYear = year;
            return View();
        }

        #endregion

        #region Create Cooperator
        [HttpPost]
        public IActionResult CreateCoop(IFormCollection form)
        {
            //add shipping unit
            ShippingUnit su = new ShippingUnit
            {
                Name = form["coopName"].ToString().Trim(),
                PhoneNumber = form["coopPhone"].ToString().Trim(),
                Email = form["coopEmail"].ToString().Trim(),
                Website = form["coopWeb"].ToString().Trim(),
                StatusId = CommonMethod.getStatusID("Cooperator", form["status"].ToString()),
            };
            String[] dateArray = form["date"].ToString().Split('-');
            if (int.Parse(dateArray[0]) >= DateTime.Now.Year && int.Parse(dateArray[1]) > DateTime.Now.Month)
            {
                su.StatusId = CommonMethod.getStatusID("Cooperator", "Inactive");
            }
            _context.ShippingUnits.Add(su);
            _context.SaveChanges();

            //add logo
            if (form["logo"].ToString().Length > 100)
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
                CommonMethod.SaveBase64Image(form["logo"].ToString(), fileName);
                _context.Images.Add(new Image
                {
                    ImageUrl = nameSaveToDB,
                    ObjectId = su.ShippingUnitId,
                    TypeId = CommonMethod.getTypeID("Cooperator")
                });
            }
            else
            {
                _context.Images.Add(new Image
                {
                    ImageUrl = "/Image/profile.jpg",
                    ObjectId = su.ShippingUnitId,
                    TypeId = CommonMethod.getTypeID("Cooperator")
                });
            }


            //add shipping prices
            int type = CommonMethod.getTypeID("Incity-normal");
            for (int i = 1; i <= 9; i++)
            {
                ShippingPrice price = new ShippingPrice
                {
                    ShippingUnitId = su.ShippingUnitId,
                    Price = decimal.Parse(form["price" + i].ToString()),
                    TypeId = type,
                    PerKm = 0,
                    StartDate = int.Parse(form["date"].ToString().Replace("-", ""))
                };
                if (i == 9)
                {
                    price.PerKm = int.Parse(form["per"].ToString());
                }
                type++;
                _context.ShippingPrices.Add(price);
            }
            _context.SaveChanges();
            return Redirect("../CooperatorManagement");
        }
        #endregion

        #region Update Cooperator
        [HttpPost]
        public IActionResult UpdateCoop(IFormCollection form)
        {
            int id = int.Parse(form["id"].ToString());
            var coop = _context.ShippingUnits
                .Where(su => su.ShippingUnitId==id)
                .FirstOrDefault();

            //get common data
            coop.Name = form["name"].ToString().Trim();
            coop.Email = form["email"].ToString().Trim();
            coop.Website = form["website"].ToString().Trim();
            coop.PhoneNumber = form["phone"].ToString().Trim();

            //get image data
            String logo = form["logo"].ToString();
            if (logo.Length > 200)
            {
                String fileName = "";
                String newAvatarUrl = "";
                String nameSaveToDB = "";
                do
                {
                    newAvatarUrl = CommonMethod.GenerateRandomString(15);
                    fileName = "wwwroot/Image/Logo/" + newAvatarUrl + ".jpg";
                    nameSaveToDB = "/Image/Logo/" + newAvatarUrl + ".jpg";
                } while (_context.Images.Where(i => i.ImageUrl == nameSaveToDB) == null);
                CommonMethod.SaveBase64Image(logo, fileName);
                var avatar = _context.Images
                                     .Where(i => i.Type.TypeName == "Cooperator"
                                     && i.ObjectId == id).FirstOrDefault();
                avatar.ImageUrl = nameSaveToDB;
            }

            //get price data
            
            String[] date = form["date[]"].ToArray();
            String[] price = form["price[]"].ToArray();
            Dictionary<int, String> priceType = new Dictionary<int, string>()
            {
                {0, "Incity-normal"},
                {1, "Incity-heavy"},
                {2, "Incity-normal-fragile"},
                {3, "Incity-heavy-fragile"},
                {4, "Outcity-normal"},
                {5, "Outcity-heavy"},
                {6, "Outcity-normal-fragile"},
                {7, "Outcity-heavy-fragile"},
                {8, "Outcity-perKM"}
            };

            for (int i = 0; i < price.Length; i++)
            {
                int now = int.Parse(DateTime.Now.Year+""+DateTime.Now.Month.ToString("D2"));
                int startDate = int.Parse(date[i].ToString().Replace("-", "").Replace("/", ""));
                if (startDate > now)
                {
                    var shippingPrice = _context.ShippingPrices
                        .Where(sp => sp.ShippingUnitId==id && sp.StartDate==startDate &&
                        sp.TypeId==CommonMethod.getTypeID(priceType[i]))
                        .FirstOrDefault();
                    if (shippingPrice!=null)
                    {
                        shippingPrice.Price = decimal.Parse(price[i].Trim());
                    }
                    else
                    {
                        _context.ShippingPrices.Add(new ShippingPrice
                        {
                            ShippingUnitId = id,
                            Price = decimal.Parse(price[i].Trim()),
                            TypeId = CommonMethod.getTypeID(priceType[i]),
                            PerKm = i == 8 ? int.Parse(form["per"].ToString()) : 0,
                            StartDate = startDate
                        });
                    }
                }
            }

            _context.SaveChanges();
            return Redirect($"../CooperatorDetails?id={id}");
        }
        #endregion

        #region Profile Manager
        public IActionResult ManagerProfile()
        {
            ViewBag.ActiveTitle = "profile";
            if (HttpContext.Session.GetString("UserRole")!="Manager")
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
            return View();
        }
        #endregion

        #region Update Profile Manager

        [HttpPost]
        public IActionResult ManagerUpdateProfile(IFormCollection form)
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
                ViewBag.phoneDuplicateAlert = "Enter phone number is existed! please enter a new one";
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

            return RedirectToAction("ManagerProfile", "Manager");
        }

        #endregion

        #region Manager statistic
        private void loadDataManagerStatistic(int year, int month)
        {
            int UserID = (int)HttpContext.Session.GetInt32("UserID");
            int[] years = { DateTime.Now.Year, DateTime.Now.Year-1,
                DateTime.Now.Year-2, DateTime.Now.Year-3 };
            int[] months = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            var totalCustomer = _context.Accounts
           .Where(r => r.RoleId == CommonMethod.getRoleID("Customer"))
           .Count();

            var totalArtist = _context.Accounts
           .Where(r => r.RoleId == CommonMethod.getRoleID("Artist"))
           .Count();

            ViewBag.totalCustomer = totalCustomer;
            ViewBag.totalArtist = totalArtist;
            ViewBag.selectedYear = year;
            ViewBag.selectedMonth = month;
            ViewBag.years = years;
            ViewBag.months = months;
        }           
            public IActionResult StatisticManager()
        {
            if (HttpContext.Session.GetString("UserRole") != "Manager")
            {
                return View("Error");
            }
            ViewBag.ActiveTitle = "statistic";
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            loadDataManagerStatistic(year, month);
            return View();
        }

        [HttpPost]
        public IActionResult StatisticManager(IFormCollection form)
        {
            int year = int.Parse(form["year"].ToString());
            int month = int.Parse(form["month"].ToString());
            loadDataManagerStatistic(year, month);
            return View();
        }

        #endregion

    }
}
