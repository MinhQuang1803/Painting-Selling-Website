using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using SEP490_G28_SP24_WebSellingPaint.FeatureCode;
using SEP490_G28_SP24_WebSellingPaint.FunctionCode;
using SEP490_G28_SP24_WebSellingPaint.Models;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Principal;


namespace SEP490_G28_SP24_WebSellingPaint.Controllers
{
    public class ArtistController : Controller
    {

        private readonly WebSellingPaintingContext _context;

        public ArtistController(WebSellingPaintingContext context)
        {
            _context = context;
        }

        #region Painting Management Artist
        public IActionResult Painting()
        {
            if (HttpContext.Session.GetString("UserRole") != "Artist")
            {
                return View("Error");
            }
            //ViewBag.ActiveTitle = "shop";
            ViewBag.ActiveTitle = "painting";
            ViewBag.searchbox = "";
            ViewBag.category = 0;
            ViewBag.status = 0;
            ViewBag.minPrice = 0;
            ViewBag.maxPrice = 99999999;
            ViewBag.minDis = 0;
            ViewBag.maxDis = 99;
            var paintings = _context.Paintings.Where(p => p.User.Account.Status.StatusName == "Enable"
            && p.UserId == (int)HttpContext.Session.GetInt32("UserID"))
                .Join(
                _context.Images.Where(i => i.Type.TypeName == "Painting"),
                painting => painting.PaintingId,
                image => image.ObjectId,
                (painting, image) => new
                {
                    Painting = painting,
                    ImageUrl = image.ImageUrl,
                    Discount = painting.Discount.Percentage
                })
                .GroupBy(p => new
                {
                    p.Painting.PaintingId,
                    p.Painting.Name,
                    p.Painting.Price,
                    p.Painting.UserId,
                    p.Painting.StatusId,
                    p.Painting.DiscountId,
                    p.Discount
                }).Select(g => new
                {
                    Painting = g.Key,
                    ImageUrl = g.FirstOrDefault().ImageUrl,
                    Discount = g.Key.Discount
                })
            .ToList();

            if (paintings.ToList().Count == 0)
            {
                ViewBag.alert = "There are no painting for now. Please Create more painting.";
            }
            else
            {
                ViewBag.PaintingList = paintings;
            }

            List<Category> categories = CommonMethod.GetActiveCategories("Painting", "Active");
            ViewBag.CategoryList = categories;

            var status = _context.Statuses.Where(s => s.Type.TypeName == "Painting").ToList();
            ViewBag.StatusList = status;
            List<Tuple<string, string>> orderbyList = new List<Tuple<string, string>>
            {
                Tuple.Create("Price", "Price"),
                Tuple.Create("Discount", "Discount")
            };
            ViewBag.orderbyList = orderbyList;
            return View();
        }

        [HttpPost]
        public IActionResult Painting(IFormCollection form)
        {
            ViewBag.ActiveTitle = "painting";
            ViewBag.searchbox = form["searchbox"].ToString().Trim();
            ViewBag.category = int.Parse(form["category"].ToString().Trim());
            ViewBag.status = int.Parse(form["status"].ToString().Trim());
            ViewBag.minPrice = int.Parse(form["minPrice"].ToString());
            ViewBag.maxPrice = int.Parse(form["maxPrice"].ToString());
            ViewBag.minDis = int.Parse(form["minDis"].ToString());
            ViewBag.maxDis = int.Parse(form["maxDis"].ToString());
            ViewBag.orderby = form["orderby"].ToString();
            ViewBag.order = form["order"].ToString();
            int statusID = int.Parse(form["status"].ToString());

            //filter by stats
            if (statusID == 0)
            {
                var paintingsWithImages = _context.Paintings.Where(p => p.User.Account.Status.StatusName == "Enable")
                .Join(
                _context.Images.Where(i => i.Type.TypeName == "Painting"),
                painting => painting.PaintingId,
                image => image.ObjectId,
                (painting, image) => new
                {
                    Painting = painting,
                    ImageUrl = image.ImageUrl,
                    Discount = painting.Discount.Percentage,
                    Status = painting.Status
                })
                .Where(p => p.Painting.Name.Contains(form["searchbox"].ToString().Trim())
                && (p.Painting.Price - p.Painting.Price * p.Discount / 100 >= decimal.Parse(form["minPrice"].ToString().Trim())
                    && p.Painting.Price - p.Painting.Price * p.Discount / 100 < decimal.Parse(form["maxPrice"].ToString().Trim()))
                && (p.Discount >= int.Parse(form["minDis"].ToString().Trim())
                    && p.Discount < int.Parse(form["maxDis"].ToString().Trim()))
                )
                .GroupBy(p => new
                {
                    p.Painting.PaintingId,
                    p.Painting.Name,
                    p.Painting.Price,
                    p.Painting.UserId,
                    p.Painting.StatusId,
                    p.Painting.DiscountId,
                    p.Discount
                })
                .Select(g => new
                {
                    Painting = g.Key,
                    ImageUrl = g.FirstOrDefault().ImageUrl,
                    Discount = g.Key.Discount
                })
            .ToList();
                int cateID = int.Parse(form["category"].ToString());

                if (cateID != 0)
                {
                    paintingsWithImages = paintingsWithImages
                        .Where(p => CommonMethod.IsCategory(p.Painting.PaintingId, cateID) == true).ToList();
                }

                if (paintingsWithImages.ToList().Count == 0)
                {
                    ViewBag.alert = "There are no painting for now. Please come back later.";
                }
                else
                {
                    //order the list
                    switch (form["orderby"].ToString())
                    {
                        case "Price":
                            paintingsWithImages = form["order"].ToString() == "ASC" ?
                                paintingsWithImages.OrderBy(p => p.Painting.Price - p.Painting.Price * p.Discount / 100).ToList() :
                                paintingsWithImages.OrderByDescending(p => p.Painting.Price - p.Painting.Price * p.Discount / 100).ToList();
                            break;
                        case "Discount":
                            paintingsWithImages = form["order"].ToString() == "ASC" ?
                                paintingsWithImages.OrderBy(p => p.Discount).ToList() :
                                paintingsWithImages.OrderByDescending(p => p.Discount).ToList();
                            break;
                    }
                    //DataTest.DisplayQueryResults(paintingsWithImages);
                    ViewBag.PaintingList = paintingsWithImages;

                }
            }
            else
            {
                var paintingsWithImages = _context.Paintings.Where(p => p.Status.StatusId == statusID
                && p.User.Account.Status.StatusName == "Enable")
                .Join(
                _context.Images.Where(i => i.Type.TypeName == "Painting"),
                painting => painting.PaintingId,
                image => image.ObjectId,
                (painting, image) => new
                {
                    Painting = painting,
                    ImageUrl = image.ImageUrl,
                    Discount = painting.Discount.Percentage,
                    Status = painting.Status
                })
                .Where(p => p.Painting.Name.Contains(form["searchbox"].ToString().Trim())
                && (p.Painting.Price - p.Painting.Price * p.Discount / 100 >= decimal.Parse(form["minPrice"].ToString().Trim())
                    && p.Painting.Price - p.Painting.Price * p.Discount / 100 < decimal.Parse(form["maxPrice"].ToString().Trim()))
                && (p.Discount >= int.Parse(form["minDis"].ToString().Trim())
                    && p.Discount < int.Parse(form["maxDis"].ToString().Trim()))
                )
                .GroupBy(p => new
                {
                    p.Painting.PaintingId,
                    p.Painting.Name,
                    p.Painting.Price,
                    p.Painting.UserId,
                    p.Painting.StatusId,
                    p.Painting.DiscountId,
                    p.Discount
                })
                .Select(g => new
                {
                    Painting = g.Key,
                    ImageUrl = g.FirstOrDefault().ImageUrl,
                    Discount = g.Key.Discount
                })
            .ToList();
                int cateID = int.Parse(form["category"].ToString());

                if (cateID != 0)
                {
                    paintingsWithImages = paintingsWithImages
                        .Where(p => CommonMethod.IsCategory(p.Painting.PaintingId, cateID) == true).ToList();
                }

                if (paintingsWithImages.ToList().Count == 0)
                {
                    ViewBag.alert = "There are no painting for now. Please come back later.";
                }
                else
                {
                    //order the list
                    switch (form["orderby"].ToString())
                    {
                        case "Price":
                            paintingsWithImages = form["order"].ToString() == "ASC" ?
                                paintingsWithImages.OrderBy(p => p.Painting.Price - p.Painting.Price * p.Discount / 100).ToList() :
                                paintingsWithImages.OrderByDescending(p => p.Painting.Price - p.Painting.Price * p.Discount / 100).ToList();
                            break;
                        case "Discount":
                            paintingsWithImages = form["order"].ToString() == "ASC" ?
                                paintingsWithImages.OrderBy(p => p.Discount).ToList() :
                                paintingsWithImages.OrderByDescending(p => p.Discount).ToList();
                            break;
                    }
                    //DataTest.DisplayQueryResults(paintingsWithImages);
                    ViewBag.PaintingList = paintingsWithImages;

                }
            }

            //filter by category
            List<Category> categories = CommonMethod.GetActiveCategories("Painting", "Active");
            ViewBag.CategoryList = categories;
            List<Status> statuses = _context.Statuses.Where(s => s.Type.TypeName == "Painting").ToList();
            ViewBag.StatusList = statuses;
            List<Tuple<string, string>> orderbyList = new List<Tuple<string, string>>
            {
                Tuple.Create("Price", "Price"),
                Tuple.Create("Discount", "Discount")
            };

            ViewBag.orderbyList = orderbyList;
            return View();
        }
        #endregion

        #region Publish Painting
        [HttpPost]
        //add a new painting
        public IActionResult CreatePainting(IFormCollection form)
        {
            ViewBag.ActiveTitle = "painting";
            String name = form["name"].ToString().Trim();
            decimal price = decimal.Parse(form["price"].ToString());
            decimal height = decimal.Parse(form["height"].ToString());
            decimal width = decimal.Parse(form["width"].ToString());
            String description = form["description"].ToString();
            int quantity = int.Parse(form["quantity"].ToString());
            int discount = int.Parse(form["discount"].ToString());
            int startDate = form["startDate"].ToString() == "" ? 0
                : int.Parse(form["startDate"].ToString().Replace("-", ""));
            int endDate = form["endDate"].ToString() == "" ? CommonMethod.GetCurrentDateAsInt()
                : int.Parse(form["endDate"].ToString().Replace("-", ""));
            String[] category = form["category"];
            String[] imageData = form["imageData"];


            //add to discount
            Discount discountDB = new Discount
            {
                Percentage = discount,
                StartDate = startDate,
                EndDate = endDate
            };
            _context.Discounts.Add(discountDB);
            _context.SaveChanges();

            //add to Painting
            Painting painting = new Painting
            {
                Name = name,
                Price = price,
                Height = height,
                Width = width,
                Description = description,
                Quantity = quantity,
                ViewCount = 0,
                PublishDate = CommonMethod.GetCurrentDateAsInt(),
                UserId = HttpContext.Session.GetInt32("UserID"),
                StatusId = CommonMethod.getStatusID("Painting", "Waiting"),
                DiscountId = discountDB.DiscountId
            };
            if (form["fragile"].ToString() == "1")
            {
                painting.IsFragile = true;
            }
            else
            {
                painting.IsFragile = false;
            }

            _context.Paintings.Add(painting);
            _context.SaveChanges();

            //add image data
            for (int i = 0; i < imageData.Length; i++)
            {
                String newAvatarUrl = "";
                String fileName = "";
                String nameSaveToDB = "";
                do
                {
                    newAvatarUrl = CommonMethod.GenerateRandomString(15);
                    fileName = "wwwroot/Image/PaintingImage/" + newAvatarUrl + ".jpg";
                    nameSaveToDB = "/Image/PaintingImage/" + newAvatarUrl + ".jpg";
                } while (_context.Images.Where(i => i.ImageUrl == nameSaveToDB) == null);
                CommonMethod.SaveBase64Image(imageData[i], fileName);
                Image paintingPic = new Image
                {
                    ImageUrl = nameSaveToDB,
                    ObjectId = painting.PaintingId,
                    TypeId = CommonMethod.getTypeID("Painting")
                };
                _context.Images.Add(paintingPic);
            }

            //add category data
            foreach (String categoryID in category)
            {
                Category cate = _context.Categories.Where(c => c.CategoryId == int.Parse(categoryID)).FirstOrDefault();
                painting.Categories.Add(cate);
            }
            _context.SaveChanges();
            return RedirectToAction("Painting", "Artist");
        }
        #endregion

        #region Painting Detail Artist
        public IActionResult PaintingDetailArtist(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Artist")
            {
                return View("Error");
            }
            ViewBag.ActiveTitle = "painting";
            var painting = _context.Paintings.Include(d => d.Discount).Where(p => p.PaintingId == id)
                .Select(p => new
                {
                    ID = p.PaintingId,
                    Name = p.Name,
                    Height = p.Height,
                    Width = p.Width,
                    Status = p.Status.StatusId,
                    StatusName = p.Status.StatusName,
                    Quantity = p.Quantity,
                    Discount = p.Discount.Percentage,
                    Description = p.Description,
                    startDiscount = p.Discount.StartDate,
                    endDiscount = p.Discount.EndDate,
                    Price = p.Price,
                    Category = p.Categories.Select(c => c.CategoryId).ToList(),
                    Categories = CommonMethod.getStringPaintingCate(p.PaintingId),
                    IsFragile = p.IsFragile
                }).FirstOrDefault();
            if (painting == null)
            {
                return RedirectToAction("Painting", "Artist");
            }
            else
            {
                String startDate = painting.Discount > 0 ? DateTime.ParseExact(painting.startDiscount.ToString(),
                    "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd") : "";
                String endDate = painting.Discount > 0 ? DateTime.ParseExact(painting.endDiscount.ToString(),
                    "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd") : "";

                var ImageList = _context.Images.Where(_image => _image.ObjectId == id
                && _image.Type.TypeName == "Painting")
                    .Select(i => new
                    {
                        ID = i.ImageId,
                        Url = i.ImageUrl
                    }).ToList();

                var category = _context.Categories.Where(_category => _category.Type.TypeName == "Painting")
                    .Select(c => new
                    {
                        ID = c.CategoryId
                        ,
                        Name = c.CategoryName
                    }).ToList();

                ViewBag.cate = category;
                ViewBag.startDate = startDate;
                ViewBag.endDate = endDate;
                ViewBag.ImageList = ImageList;
                ViewBag.painting = painting;
            }
            return View();
        }
        #endregion

        #region Update Painting
        [HttpPost]
        public IActionResult UpdatePainting(IFormCollection form)
        {
            int pid = int.Parse(form["id"].ToString());
            decimal price = decimal.Parse(form["price"].ToString());
            String description = form["description"].ToString();
            int quantity = int.Parse(form["quantity"].ToString());
            int discount = int.Parse(form["discount"].ToString());
            int startDate = form["startDate"].ToString() == "" ? 0
                : int.Parse(form["startDate"].ToString().Replace("-", ""));
            int endDate = form["endDate"].ToString() == "" ? CommonMethod.GetCurrentDateAsInt()
                : int.Parse(form["endDate"].ToString().Replace("-", ""));
            String[] category = form["category"];

            //update common infor
            var painting = _context.Paintings.Where(p => p.PaintingId == pid).FirstOrDefault();
            painting.Price = price;
            painting.Description = description;
            painting.Quantity = quantity;
            if (form["fragile"].ToString() == "1")
            {
                painting.IsFragile = true;
            }

            //update discount
            var discountUpdate = _context.Discounts.Where(d => d.DiscountId == painting.DiscountId).FirstOrDefault();
            discountUpdate.Percentage = discount;
            if (discount > 0)
            {
                discountUpdate.StartDate = startDate;
                discountUpdate.EndDate = endDate;
            }

            //update categories
            var cateList = _context.Paintings.Include(t => t.Categories)
                .FirstOrDefault(p => p.PaintingId == pid)?.Categories;
            cateList.Clear();
            foreach (String item in category)
            {
                painting.Categories.Add(_context.Categories.Where(c => c.CategoryId == int.Parse(item)).FirstOrDefault());
            }

            return Redirect("../PaintingDetailArtist?id=" + pid);
        }

        //method to hidden painting
        [HttpPost]
        public IActionResult HideShowPainting(IFormCollection form)
        {
            int pid = int.Parse(form["id"].ToString());
            String action = form["action"].ToString();
            var painting = _context.Paintings.Where(p => p.PaintingId == pid).FirstOrDefault();
            if (action == "hide")
            {
                painting.StatusId = CommonMethod.getStatusID("Painting", "Hidden");
            }
            else
            {
                painting.StatusId = CommonMethod.getStatusID("Painting", "Active");
            }
            _context.SaveChanges();
            return Redirect("../PaintingDetailArtist?id=" + pid);
        }

        #endregion

        #region Artist Statistic

        private void loadDataArtistStatistic(int year, int month)
        {
            ViewBag.ActiveTitle = "statistic";
            int UserID = (int)HttpContext.Session.GetInt32("UserID");
            int[] years = { DateTime.Now.Year, DateTime.Now.Year-1,
                DateTime.Now.Year-2, DateTime.Now.Year-3 };
            int[] months = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            //total view
            var totalView = _context.Paintings
            .Where(p => p.UserId == UserID)
            .Sum(p => p.ViewCount);

            //total saled painting
            var totalQuantityList = _context.OrderDetails
                .Where(od => od.Painting.UserId==UserID && od.Order.Status.StatusName=="Successfully"
                && (od.Order.ShipDate/10000==year && od.Order.ShipDate/100%100==month))
                .Select(od => od.Quantity)
                .ToList();
            int totalQuantity = 0;
            foreach (var item in totalQuantityList)
            {
                totalQuantity+=(int)item;
            }

            //total transaction
            var orderIDs = _context.OrderDetails
                .Where(od => od.Painting.UserId==UserID
                && (od.Order.OrderDate/100%100==month&&od.Order.OrderDate/10000==year))
                .ToList();

            //total feedback
            var commentCount = _context.Comments
            .Join(_context.Paintings,
            c => c.ObjectId,
            p => p.PaintingId,
            (c, p) => new { Comment = c, Painting = p })
            .Join(_context.Types,
            joined => joined.Comment.TypeId,
            t => t.TypeId,
            (joined, t) => new { Joined = joined, Type = t })
            .Where(joined => joined.Joined.Painting.UserId == UserID &&
                     joined.Type.TypeName == "Painting" &&
                     joined.Joined.Comment.CommentDate.HasValue &&
                     joined.Joined.Comment.CommentDate.Value.Year == year &&
                     joined.Joined.Comment.CommentDate.Value.Month == month)
            .Count();

            //get data for success order
            String[] successStatus = { "Successfully" };
            int[] successOrder =
            {
                CommonMethod.getOrderPerMonth(year, month, UserID, 1, 7, successStatus),
                CommonMethod.getOrderPerMonth(year, month, UserID, 8, 14, successStatus),
                CommonMethod.getOrderPerMonth(year, month, UserID, 15, 21, successStatus),
                CommonMethod.getOrderPerMonth(year, month, UserID, 22, DateTime.DaysInMonth(year, month), successStatus)
            };

            //get data for not not success order
            String[] failStatus = { "Canceled", "Returned" };
            int[] failOrder =
            {
                CommonMethod.getOrderPerMonth(year, month, UserID, 1, 7, failStatus),
                CommonMethod.getOrderPerMonth(year, month, UserID, 8, 14, failStatus),
                CommonMethod.getOrderPerMonth(year, month, UserID, 15, 21, failStatus),
                CommonMethod.getOrderPerMonth(year, month, UserID, 22, DateTime.DaysInMonth(year, month), failStatus)
            };

            //get data for income charts
            decimal[] incomePerMonth =
            {
                CommonMethod.getTotalPricePerWeeks(year, month, UserID, 1, 7),
                CommonMethod.getTotalPricePerWeeks(year, month, UserID, 8, 14),
                CommonMethod.getTotalPricePerWeeks(year, month, UserID, 15, 21),
                CommonMethod.getTotalPricePerWeeks(year, month, UserID, 22, DateTime.DaysInMonth(year, month)),
            };

            ViewBag.successOrder = successOrder;
            ViewBag.failOrder = failOrder;
            ViewBag.incomePerMonth = incomePerMonth;
            ViewBag.feedbackCount = commentCount;
            ViewBag.totalOrder = orderIDs.Count();
            ViewBag.totalQuantity = totalQuantity;
            ViewBag.totalView = totalView;
            ViewBag.selectedYear = year;
            ViewBag.selectedMonth = month;
            ViewBag.years = years;
            ViewBag.months = months;
        }

        [HttpGet]
        public IActionResult StatisticArtist()
        {
            if (HttpContext.Session.GetString("UserRole") != "Artist")
            {
                return View("Error");
            }
            ViewBag.ActiveTitle = "statistic";
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            loadDataArtistStatistic(year, month);
            return View();
        }

        [HttpPost]
        public IActionResult StatisticArtist(IFormCollection form)
        {
            int year = int.Parse(form["year"].ToString());
            int month = int.Parse(form["month"].ToString());
            loadDataArtistStatistic(year, month);
            return View();
        }

        #endregion

        #region ArtistPromotion
        //method to load data for promotion artist page
        private void loadDataForPromotionArtist(int startDate, int endDate, decimal minReduce, decimal maxReduce,
            int status, decimal fromMinOrderPrice, decimal toMinOrderPrice)
        {
            int userID = (int)HttpContext.Session.GetInt32("UserID");
            var promotionList = _context.Vouchers
                .Where(u => u.UserId == userID
                && (u.StartDate >= startDate && u.StartDate <= endDate)
                && (u.Percentage >= minReduce && u.Percentage <= maxReduce)
                && (u.MinOrderValue >= fromMinOrderPrice && u.MinOrderValue <= toMinOrderPrice))
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

        public IActionResult Promotion()
        {
            ViewBag.ActiveTitle = "promotion";
            if (HttpContext.Session.GetString("UserRole") != "Artist")
            {
                return View("Error");
            }
            decimal min = 0;
            decimal max = 999999999;
            loadDataForPromotionArtist((int)min, (int)max, min, max, (int)min, min, max);
            ViewBag.StartDateError = TempData["CreatePromotionError"];
            return View();
        }

        [HttpPost]
        public IActionResult Promotion(IFormCollection form)
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

            loadDataForPromotionArtist(startDate, endDate, minReduce, maxReduce,
                status, minOrderPrice, maxOrderPrice);

            ViewBag.startDate = startDate == 0 ? "" : form["activeFrom"].ToString();
            ViewBag.endDate = endDate == 999999999 ? "" : form["activeTo"].ToString();
            ViewBag.minReduce = minReduce == 0 ? "" : form["reduceFrom"].ToString();
            ViewBag.maxReduce = maxReduce == 999999999 ? "" : form["reduceTo"].ToString();
            ViewBag.status = status;
            ViewBag.minOrderPrice = minOrderPrice == 0 ? "" : form["minOrderPrice"].ToString();
            ViewBag.maxOrderPrice = maxOrderPrice == 999999999 ? "" : form["maxOrderPrice"].ToString();

            return View();
        }
        #endregion

        #region Create promotion
        [HttpPost]
        public IActionResult CreatePromotion(IFormCollection form)
        {
            ViewBag.ActiveTitle = "promotion";
            string promotionCode = CommonMethod.GenerateRandomString(15);
            string promotionName = form["PromotionName"].ToString().Trim();
            string status = form["status"].ToString();
            int promotionStatus;

            if (status == "Active")
            {
                promotionStatus = CommonMethod.getStatusID("Promotion", "Active");
            }
            else
            {
                promotionStatus = CommonMethod.getStatusID("Promotion", "Inactive");
            }

            int startDate = form["StartDate"].ToString() == "" ? 0
            : int.Parse(form["StartDate"].ToString().Replace("-", ""));
            int endDate = form["EndDate"].ToString() == "" ? CommonMethod.GetCurrentDateAsInt()
                : int.Parse(form["EndDate"].ToString().Replace("-", ""));
            decimal discount = decimal.Parse(form["Discount"]);
            decimal minValue = decimal.Parse(form["MinValue"]);

            string promotionLogo = form["logo"].ToString();

            int UserID = (int)HttpContext.Session.GetInt32("UserID");

            Voucher voucher = new Voucher
            {
                VoucherCode = promotionCode,
                VoucherName = promotionName,
                StartDate = startDate,
                EndDate = endDate,
                Percentage = discount,
                MinOrderValue = minValue,
                UserId = UserID,
                StatusId = promotionStatus
            };

            _context.Add(voucher);
            _context.SaveChanges();

            if (promotionLogo.Length > 200)
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
                CommonMethod.SaveBase64Image(promotionLogo, fileName);
                var avatar = _context.Images.Where(i => i.Type.TypeName == "Promotion"
                                     && i.ObjectId == voucher.VoucherId).FirstOrDefault();
                _context.Images.Add(new Image
                {
                    ImageUrl = nameSaveToDB,
                    ObjectId = voucher.VoucherId,
                    TypeId = CommonMethod.getTypeID("Promotion")
                });

            }
            else
            {
                _context.Images.Add(new Image
                {
                    ImageUrl = promotionLogo,
                    ObjectId = voucher.VoucherId,
                    TypeId = CommonMethod.getTypeID("Promotion")
                });
            }
            _context.SaveChanges();
            return RedirectToAction("Promotion", "Artist");
        }
        #endregion

        #region Promotion Detail Artist
        public IActionResult PromotionDetailArtist(int id)
        {
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
            if (promotionDetail == null | HttpContext.Session.GetString("UserRole") != "Artist")
            {
                return View("Error");
            }

            ViewBag.promotion = promotionDetail;
            return View();
        }
        #endregion

        #region Artist Profile

        public IActionResult ProfileArtist()
        {
            ViewBag.ActiveTitle = "profile";
            if (HttpContext.Session.GetString("UserRole") != "Artist")
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
            var address = _context.Addresses
                .Join(_context.Types, a => a.TypeId, t => t.TypeId, (a, t) => new { Address = a, Type = t })
                .Join(_context.Users, at => at.Address.ObjectId, u => u.UserId, (at, u) => new { at.Address, at.Type, User = u })
                .Join(_context.Statuses, aut => aut.Address.Status, s => s.StatusId, (aut, s) => new { aut.Address, aut.Type, aut.User, Status = s })
                .Where(auts => auts.Type.TypeName == "User" &&
                               auts.Address.ObjectId == userID &&
                               auts.User.UserId == userID &&
                               auts.Status.StatusName != "Inactive")
                .OrderBy(auts => auts.Status.StatusId)
                .Select(auts => new
                {
                    auts.Address.AddressId,
                    auts.Address.City,
                    auts.Address.District,
                    auts.Address.Street,
                    auts.Address.Detail,
                    auts.Status.StatusName,
                    auts.Status.StatusId
                })
                .ToList();

            ViewBag.address = address;
            ViewBag.user = user;
            return View();
        }

        //method to changes artist information
        [HttpPost]
        public IActionResult ChangeProfile(IFormCollection form)
        {
            String imageData = form["imageData"].ToString();
            String name = form["username"].ToString().Trim();
            String phone = form["phonenumber"].ToString().Trim();
            int userid = (int)HttpContext.Session.GetInt32("UserID");
            String genderString = form["gender"].ToString();
            bool gender = genderString == "1" ? true : false;
            String background = form["background"].ToString().Trim();
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
            user.ArtistBackground = background;
            _context.SaveChanges();
            return Redirect("../ProfileArtist");
        }
        #endregion

        #region Artist Order Management

        //method to load data for order management artist 
        private void loadOrderManagementArtist(int status, int startDate, int endDate)
        {
            //load order data
            int userID = (int)HttpContext.Session.GetInt32("UserID");
            var orderList = _context.OrderDetails.Include(t => t.Order)
                .Where(o => o.Painting.UserId == userID
                && (o.Order.OrderDate >= startDate && o.Order.OrderDate < endDate))
                .Select(o => new
                {
                    ID = o.Order.OrderId,
                    OrderDate = DateTime.ParseExact(o.Order.OrderDate.ToString(), "yyyyMMdd", null)
                    .ToString("dd/MM/yyyy"),
                    StatusID = o.Order.StatusId,
                    StatusName = o.Order.Status.StatusName,
                    Date = o.Order.OrderDate
                })
                .GroupBy(o => o.ID)
                .Select(o => o.First())
                .ToList();
            orderList = orderList.OrderByDescending(od => od.Date).ThenByDescending(od => od.ID).ToList();

            if (status > 0)
            {
                orderList = orderList.Where(o => o.StatusID == status).ToList();
            }

            //load status data
            var statusList = _context.Statuses
                .Where(s => s.Type.TypeName == "Order")
                .Select(s => new
                {
                    ID = s.StatusId,
                    Name = s.StatusName
                }).ToList();

            ViewBag.status = status;
            ViewBag.statusList = statusList;
            ViewBag.orderList = orderList;
        }

        [HttpGet]
        public IActionResult OrderArtist()
        {
            ViewBag.ActiveTitle = "order";
            if (HttpContext.Session.GetString("UserRole") != "Artist")
            {
                return View("Error");
            }
            loadOrderManagementArtist(0, 0, 99999999);
            return View();
        }

        [HttpPost]
        public IActionResult OrderArtist(IFormCollection form)
        {
            int status = int.Parse(form["status"].ToString());
            int startDate = form["startDate"].ToString() == "" ? 0 :
                int.Parse(form["startDate"].ToString().Replace("-", ""));
            int endDate = form["endDate"].ToString() == "" ? 99999999 :
                int.Parse(form["endDate"].ToString().Replace("-", ""));
            loadOrderManagementArtist(status, startDate, endDate);

            ViewBag.startDate = startDate == 0 ? "" : form["startDate"].ToString();
            ViewBag.endDate = endDate == 99999999 ? "" : form["endDate"].ToString();
            return View();
        }
        #endregion

        #region Artist Order Detail
        //method to get the detail of an order
        public IActionResult OrderDetailArtist(int id)
        {
            ViewBag.ActiveTitle = "order";
            int userID = (int)HttpContext.Session.GetInt32("UserID");

            var details = _context.OrderDetails.Include(t => t.Painting)
                .Where(od => od.OrderId == id &&
                od.Painting.UserId == userID)
                .Select(od => new
                {
                    ID = od.OrderDetailId,
                    Name = od.Painting.Name,
                    Quantity = od.Quantity,
                    Total = CommonMethod.getActualPrice(od.Price, od.Quantity, od.Discount)
                    .ToString("#,0", CultureInfo.InvariantCulture),
                    Status = od.Status.StatusName,
                    Price = CommonMethod.getActualPrice(od.Price, 1, 0)
                    .ToString("#,0", CultureInfo.InvariantCulture),
                    Discount = od.Discount,
                }).ToList();

            var orderStatus = _context.Orders.Include(s => s.Status)
                .SingleOrDefault(o => o.OrderId == id);

            ViewBag.orderStatus = orderStatus.Status.StatusName;
            ViewBag.orderId = orderStatus.OrderId;

            if (details == null | HttpContext.Session.GetString("UserRole") != "Artist")
            {
                return View("Error");
            }

            ViewBag.details = details;
            return View();
        }

        //method for update status of an order detail
        [HttpPost]
        public IActionResult MarkAsDelivering(IFormCollection form)
        {
            int id = int.Parse(form["id"].ToString());
            //change the status of current order detail
            var detail = _context.OrderDetails
                .Where(od => od.OrderDetailId == id)
                .FirstOrDefault();
            detail.StatusId = CommonMethod.getStatusID("OrderDetail", "Delivering");
            _context.SaveChanges();

            //check and update order status
            var order = _context.Orders.Include(t => t.Status)
                .Where(o => o.OrderId == detail.OrderId)
                .FirstOrDefault();
            var od = _context.OrderDetails
                .Where(od => od.Status.StatusName == "Processing" && od.OrderId == order.OrderId)
                .ToList();
            if (od.Count == 0)
            {
                order.StatusId = CommonMethod.getStatusID("Order", "Delivering");
            }

            _context.SaveChanges();
            return Redirect($"../OrderDetailArtist?id={order.OrderId}");
        }

        #endregion

        #region Order Action

        [HttpPost]
        public IActionResult MarkOrder(IFormCollection form)
        {
            int id = int.Parse(form["id"].ToString());
            string typeAction = form["action"].ToString();
            var now = DateTime.Now;

            var currentOrder = _context.Orders.SingleOrDefault(o => o.OrderId == id);
            var getAllOrderDetail = _context.OrderDetails.Where(o => o.OrderId == id);

            if (currentOrder != null)
            {
                if (typeAction == "Complete")
                {
                    currentOrder.StatusId = CommonMethod.getStatusID("Order", "Successfully");
                    currentOrder.ShipDate = int.Parse($"{now.Year}{now.Month.ToString("D2")}{now.Day.ToString("D2")}");
                    foreach (var items in getAllOrderDetail)
                    {
                        items.StatusId = CommonMethod.getStatusID("OrderDetail", "Delivered");
                        _context.Update(items);
                    }
                }
                else
                {
                    currentOrder.StatusId = CommonMethod.getStatusID("Order", "Canceled");
                    foreach (var items in getAllOrderDetail)
                    {
                        items.StatusId = CommonMethod.getStatusID("OrderDetail", "Returned");
                        _context.Update(items);
                    }
                }
                _context.SaveChanges();
            }
            return Redirect($"../OrderDetailArtist?id={id}");
        }

        #endregion

        #region Artist Update Promotion

        [HttpPost]
        public IActionResult UpdatePromotionArtist(IFormCollection form)
        {
            return RedirectToAction("PromotionDetailArtist", "Artist");

        }

        #endregion

        #region Artist Active Account Payment

        private void removeSession(int type)
        {
            var session = HttpContext.Session;
            if (type == 1)
            {
                session.Remove("activeCode");
                session.Remove("userDistrict");
                session.Remove("userWards");
                session.Remove("userCity");
                session.Remove("ArtistBackground");
                session.Remove("userAddressDetail");
                session.Remove("Password");
                session.Remove("userEmail");
                session.Remove("UserName");
                session.Remove("ShopName");
                session.Remove("userGender");
                session.Remove("UserPhone");
            }
            else
            {
                session.Remove("extendCode");
            }

        }


        private static int convertDateTime(DateTime date)
        {
            string result = date.ToString("yyyy/MM/dd");
            int ConvertDateToInt = result.ToString() == "" ? 0
                          : int.Parse(result.ToString().Replace("/", ""));
            return ConvertDateToInt;
        }


        public IActionResult ActiveAccountPayment()
        {
            DateTime todayDate = DateTime.Today;
            DateTime expiredDate = todayDate.AddMonths(1);
            int saveActiveDate = convertDateTime(todayDate);
            int saveExpiredDate = convertDateTime(expiredDate);
            string convertTodaydate = todayDate.ToString("dd/MM/yyyy");
            string convertExpiredDate = expiredDate.ToString("dd/MM/yyyy");
            string userCode = HttpContext.Session.GetString("activeCode");
            decimal activeAmount = 300000;
            var paymentQRUrl = "https://img.vietqr.io/image/VCB-9926056888-compact.png?accountName=NGHIEM%20HUY%20SON&addInfo="
          + userCode + "&amount=" + activeAmount;

            ViewBag.QrGenerate = paymentQRUrl;
            ViewBag.UpgradeAccountCode = userCode;

            var getTypetransaction = _context.Types.SingleOrDefault(t => t.TypeName == "UpgradeArtistPay");

            var checkHistoryTransaction = _context.TransactionHistories.SingleOrDefault(s => s.Content == userCode
            && s.TypeId == getTypetransaction.TypeId && s.Amount == activeAmount);

            var getActivePayAccountStatus = _context.Statuses.SingleOrDefault(s => s.StatusName == "Paid And Active");

            if (checkHistoryTransaction != null)
            {
                ViewBag.success = "ok";

                Account account = new Account
                {
                    Email = HttpContext.Session.GetString("userEmail"),
                    Password = HttpContext.Session.GetString("Password"),
                    RoleId = CommonMethod.getRoleID("Artist"),
                    StatusId = CommonMethod.getStatusID("Account", "Enable")
                };
                _context.Accounts.Add(account);
                _context.SaveChanges();


                int userGender = (int)HttpContext.Session.GetInt32("userGender");
                bool insertGender = true;
                if (userGender == 0)
                {
                    insertGender = false;
                }

                User newActiveUser = new User
                {
                    UserName = HttpContext.Session.GetString("UserName"),
                    ShopName = HttpContext.Session.GetString("ShopName"),
                    ArtistBackground = HttpContext.Session.GetString("ArtistBackground"),
                    PhoneNumber = HttpContext.Session.GetString("UserPhone"),
                    Gender = insertGender,
                    AccountId = account.AccountId
                };

                _context.Users.Add(newActiveUser);
                _context.SaveChanges();

                Address userAddress = new Address
                {
                    City = HttpContext.Session.GetString("userCity"),
                    District = HttpContext.Session.GetString("userDistrict"),
                    Street = HttpContext.Session.GetString("userWards"),
                    Detail = HttpContext.Session.GetString("userAddressDetail"),
                    Status = CommonMethod.getStatusID("Address", "Default"),
                    ObjectId = newActiveUser.UserId,
                    TypeId = CommonMethod.getTypeID("User")
                };

                _context.Addresses.Add(userAddress);
                _context.SaveChanges();

                var newAvatar = new Image()
                {
                    ImageUrl = "/Image/User/avatar.jpg",
                    ObjectId = newActiveUser.UserId,
                    TypeId = CommonMethod.getTypeID("User")
                };
                _context.Images.Add(newAvatar);
                _context.SaveChanges();


                PayAccount activeAccount = new PayAccount
                {
                    ActiveAccountDate = saveActiveDate,
                    ExpiredAccountDate = saveExpiredDate,
                    ActiveCode = userCode,
                    ActiveFee = activeAmount,
                    AccountId = account.AccountId,
                    StatusId = CommonMethod.getStatusID("Account", "Paid And Active")
                };

                _context.PayAccounts.Add(activeAccount);
                _context.SaveChanges();

                UserTransaction newUserTransaction = new UserTransaction
                {
                    TransactionId = checkHistoryTransaction.TransactionId,
                    UserId = newActiveUser.UserId,
                    PayAccountId = activeAccount.PayAccountId
                };

                _context.UserTransactions.Add(newUserTransaction);
                _context.SaveChanges();
                removeSession(1);
                ViewBag.ActiveDate = convertTodaydate;
                ViewBag.ExpiredDate = convertExpiredDate;
            }
            else
            {
                ViewBag.ActiveDate = convertTodaydate;
                ViewBag.ExpiredDate = convertExpiredDate;
            }
            return View();
        }

        public IActionResult ReActiveAccountPayment()
        {
            var accountIdToExtend = (int)HttpContext.Session.GetInt32("AccountID");
            var userId = (int)HttpContext.Session.GetInt32("UserID");
            var getPayAccountPayment = _context.PayAccounts.SingleOrDefault(p => p.AccountId == accountIdToExtend);
            DateTime todayDate = DateTime.Today;
            DateTime expiredDate = todayDate.AddMonths(1);

            int saveActiveDate = convertDateTime(todayDate);
            int saveExpiredDate = convertDateTime(expiredDate);

            string convertTodaydate = todayDate.ToString("dd/MM/yyyy");
            string convertExpiredDate = expiredDate.ToString("dd/MM/yyyy");
            string extendAccountCode = getPayAccountPayment.ActiveCode;

            decimal? upgradeAmount = getPayAccountPayment.ActiveFee;
            var paymentQRUrl = "https://img.vietqr.io/image/VCB-9926056888-compact.png?accountName=NGHIEM%20HUY%20SON&addInfo="
           + extendAccountCode + "&amount=" + upgradeAmount;
            ViewBag.QrGenerate = paymentQRUrl;
            ViewBag.extendAccountCode = extendAccountCode;

            var getTypetransaction = _context.Types.SingleOrDefault(t => t.TypeName == "UpgradeArtistPay");

            var checkHistoryTransaction = _context.TransactionHistories.SingleOrDefault(s => s.Content == extendAccountCode
            && s.TypeId == getTypetransaction.TypeId && s.Amount == upgradeAmount);

            if (checkHistoryTransaction != null)
            {
                ViewBag.success = "ok";
                ViewBag.ActiveDate = convertTodaydate;
                ViewBag.ExpiredDate = convertExpiredDate;
                getPayAccountPayment.ActiveAccountDate = saveActiveDate;
                getPayAccountPayment.ExpiredAccountDate = saveExpiredDate;
                getPayAccountPayment.ActiveCode = extendAccountCode;
                getPayAccountPayment.StatusId = CommonMethod.getStatusID("Account", "Paid And Active");
                UserTransaction newUserTransaction = new UserTransaction
                {
                    TransactionId = checkHistoryTransaction.TransactionId,
                    UserId = userId,
                    PayAccountId = getPayAccountPayment.PayAccountId
                };
                _context.SaveChanges();
                removeSession(2);
            }
            else
            {
                ViewBag.ActiveDate = convertTodaydate;
                ViewBag.ExpiredDate = convertExpiredDate;
            }
            return View();
        }
        #endregion
    }

}
