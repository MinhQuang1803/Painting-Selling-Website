using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Debugger.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SEP490_G28_SP24_WebSellingPaint.FeatureCode;
using SEP490_G28_SP24_WebSellingPaint.FunctionCode;
using SEP490_G28_SP24_WebSellingPaint.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static System.Collections.Specialized.BitVector32;

namespace SEP490_G28_SP24_WebSellingPaint.Controllers
{
    public class CustomerController : Controller
    {
        WebSellingPaintingContext _context;

        public CustomerController(WebSellingPaintingContext context)
        {
            _context = context;
        }

        #region AddTransactionsAPI 
        [HttpPost]
        public IActionResult AddTransactionsToDataBase(string id, string time, string content, string money, string paidItems)
        {

            var getTransactionsPaidItems = _context.Types.SingleOrDefault(t => t.TypeName == paidItems);

            var getTransactionsStatus = _context.Statuses.SingleOrDefault(s => s.StatusName == "Transaction Completed" && s.TypeId == getTransactionsPaidItems.TypeId);

            string transContent = convertPaymentCode(content);

            int timeConvert = time.ToString() == "" ? 0
            : int.Parse(time.ToString().Replace("/", ""));


            var checkDupTransactions = _context.TransactionHistories.SingleOrDefault(h => h.PaymentId == id);

            if (checkDupTransactions == null)
            {
                TransactionHistory historyTransaction = new TransactionHistory
                {
                    PaymentId = id,
                    Time = timeConvert,
                    Content = transContent,
                    Amount = decimal.Parse(money),
                    TypeId = getTransactionsPaidItems.TypeId,
                    StatusId = getTransactionsStatus.StatusId
                };
                _context.TransactionHistories.Add(historyTransaction);
                _context.SaveChanges();
            }
            return Ok();
        }

        private static string convertPaymentCode(string content)
        {

            var upgradeArtistmatch = Regex.Match(content, "WSPU[A-Z0-9]{10}");

            if (upgradeArtistmatch.Success)
            {
                return upgradeArtistmatch.Value;
            }

            return "";
        }
        #endregion

        #region Home
        public IActionResult Index()
        {
            ViewBag.ActiveTitle = "home";
            //get categories
            var categories = _context.Categories.Where(c => c.Type.TypeName == "Painting" && c.Status.StatusName == "Active")
                .Select(c => new
                {
                    ID = c.CategoryId,
                    Name = c.CategoryName,
                    Description = c.Description,
                    FirstPic = CommonMethod.getFirstImage(c.Paintings.FirstOrDefault().PaintingId)
                }).Take(5).ToList();

            //get paintings
            var paintingsWithImages = _context.Paintings.Where(p => p.Status.StatusName == "Active"
                && p.User.Account.Status.StatusName == "Enable")
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
                    p.Painting.Description,
                    p.Painting.Height,
                    p.Painting.Width,
                    p.Painting.Quantity,
                    p.Painting.ViewCount,
                    p.Painting.PublishDate,
                    p.Painting.UserId,
                    p.Painting.StatusId,
                    p.Painting.DiscountId,
                    p.Discount
                })
                .Select(g => new
                {
                    Painting = g.Key,
                    ImageUrl = g.FirstOrDefault().ImageUrl,
                    Discount = g.Key.Discount,
                    Price = CommonMethod.getActualPrice(g.Key.Price, 1, 0)
                    .ToString("#,0", CultureInfo.InvariantCulture),
                    DiscountPrice = CommonMethod.getActualPrice(g.Key.Price, 1, g.Key.Discount)
                    .ToString("#,0", CultureInfo.InvariantCulture)
                })
                .OrderBy(p => p.Discount).Take(6)
            .ToList();

            //get posts
            var postList = _context.Posts.Include(p => p.Categories)
                           .Where(p => p.Status.StatusName == "Active")
                           .Select(p => new
                           {
                               ID = p.PostId,
                               Title = p.Title,
                               Date = CommonMethod.intDateToStringDate(p.Date),
                               Content = p.Content.Substring(0, Math.Min(p.Content.Length, 100)) + "...",
                               ImageUrl = _context.Images
                                   .Where(i => i.ObjectId == p.PostId && i.TypeId == CommonMethod.getTypeID("Post"))
                                   .OrderBy(i => i.ImageId)
                                   .Select(i => i.ImageUrl)
                                   .FirstOrDefault(),
                               UserName = p.User.UserName,
                               Avatar = _context.Images
                                .Where(i => i.ObjectId == p.UserId && i.TypeId == CommonMethod.getTypeID("User"))
                                .OrderBy(i => i.ImageId)
                                   .Select(i => i.ImageUrl)
                                   .FirstOrDefault()
                           }).Where(p => p.ImageUrl != null).Take(4)
                           .ToList();

            ViewBag.painting = paintingsWithImages;
            ViewBag.post = postList;
            ViewBag.cate = categories;
            return View();
        }
        #endregion

        #region CustomerProfilepage
        [HttpPost]
        public IActionResult Update(IFormCollection form)
        {
            String imageData = form["imageData"].ToString();
            String name = form["username"].ToString().Trim();
            String phone = form["phonenumber"].ToString().Trim();
            int userid = int.Parse(form["id"].ToString());
            String genderString = form["gender"].ToString();
            bool gender = genderString == "male" ? true : false;
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
            return RedirectToAction("Profile", "Customer");
        }

        [HttpPost]
        public IActionResult SetDefaultAddress(IFormCollection form)
        {
            var addressID = int.Parse(form["addressid"].ToString());
            var action = form["action"].ToString();
            var userID = HttpContext.Session.GetInt32("UserID");
            var setAddress = _context.Addresses
                             .Where(a => a.AddressId == addressID).FirstOrDefault();
            if (action == "set")
            {
                var defaultAddress = _context.Addresses
                                 .Where(a => a.StatusNavigation.StatusName == "Default"
                                 && a.ObjectId == userID
                                 && a.Type.TypeName == "User").FirstOrDefault();
                defaultAddress.Status = CommonMethod.getStatusID("Address", "Active");
                setAddress.Status = CommonMethod.getStatusID("Address", "Default");
            }
            else setAddress.Status = CommonMethod.getStatusID("Address", "Inactive");

            _context.SaveChanges();
            return RedirectToAction("Profile", "Customer");
        }

        private void getProfileData(int postStatus, int startDate, int endDate)
        {
            ViewBag.ActiveTitle = "profile";
            ViewBag.startDate = "2000-01-01";
            ViewBag.endDate = DateTime.Today.ToString("yyyy-MM-dd");
            //get profile data
            var userID = HttpContext.Session.GetInt32("UserID");
            var profile = _context.Users.Include(u => u.Account)
                                  .Join(_context.Images.Include(i => i.Type),
                                  u => u.UserId,
                                  i => i.ObjectId,
                                  (u, i) => new
                                  {
                                      ID = u.UserId,
                                      Name = u.UserName,
                                      Email = u.Account.Email,
                                      Phone = u.PhoneNumber,
                                      Gender = u.Gender,
                                      Type = i.Type.TypeName,
                                      Avatar = i.ImageUrl
                                  }).FirstOrDefault(p => p.ID == userID && p.Type == "User");
            //get address data
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

            //get post data
            var postList = _context.Posts.Include(p => p.Status)
                       .Where(p => p.UserId == userID &&
                       p.Date > startDate && p.Date < endDate)
                       .Select(p => new
                       {
                           ID = p.PostId,
                           Title = p.Title,
                           Date = CommonMethod.intDateToStringDate(p.Date),
                           StatusID = p.StatusId,
                           StatusName = p.Status.StatusName
                       })
                       .ToList();
            if (postStatus > 0)
            {
                postList = postList.Where(p => p.StatusID == postStatus).ToList();
            }

            //get post status data
            var statusList = _context.Statuses.Where(s => s.Type.TypeName == "Post")
                             .Select(s => new
                             {
                                 ID = s.StatusId,
                                 Name = s.StatusName
                             }).ToList();

            ViewBag.statusList = statusList;
            ViewBag.postList = postList;
            ViewBag.address = address;
            ViewBag.profile = profile;
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            if (_context.Users.Where(u => u.UserId == userID).FirstOrDefault() == null
                | HttpContext.Session.GetString("UserRole") != "Customer")
            {
                return View("Error");
            }
            ViewBag.alertMess = TempData["alertMess"];
            getProfileData(0, 0, 99999999);
            return View();
        }

        [HttpPost]
        public IActionResult Profile(IFormCollection form)
        {
            ViewBag.ActiveTitle = "profile";
            int statusID = int.Parse(form["status"].ToString());
            int startDate = form["startDate"].ToString() != "" ?
                            int.Parse(form["startDate"].ToString().Replace("-", "")) : 0;
            int endDate = form["endDate"].ToString() != "" ?
                            int.Parse(form["endDate"].ToString().Replace("-", "")) : 99999999;
            getProfileData(statusID, startDate, endDate);

            ViewBag.startDate = startDate == 0 ? "2000-01-01" : form["startDate"].ToString();
            ViewBag.endDate = endDate == 99999999 ? DateTime.Today.ToString("yyyy-MM-dd") : form["endDate"].ToString();
            ViewBag.status = statusID;

            return View();
        }

        [HttpPost]
        public IActionResult ProfileAddNewAddress(IFormCollection form)
        {
            String city = form["userCity"].ToString().Trim();
            String district = form["userDistrict"].ToString().Trim();
            String ward = form["userWards"].ToString().Trim();
            String detail = form["detail"].ToString().Trim();

            var userID = HttpContext.Session.GetInt32("UserID");

            var checkDupAddress = _context.Addresses.SingleOrDefault(a => a.ObjectId == userID && a.City == city && a.Detail == detail
            && a.District == district && a.Street == ward);

            if (_context.Users.Where(u => u.UserId == userID).FirstOrDefault() == null
                | HttpContext.Session.GetString("UserRole") != "Customer")
            {
                return View("Error");
            }

            if (checkDupAddress != null)
            {
                TempData["alertMess"] = "not";
            }
            else
            {
                _context.Addresses.Add(new Address
                {
                    City = city,
                    District = district,
                    Street = ward,
                    Detail = detail,
                    Status = CommonMethod.getStatusID("Address", "Active"),
                    ObjectId = userID,
                    TypeId = CommonMethod.getTypeID("User")
                });
                _context.SaveChanges();
                TempData["alertMess"] = "ok";
            }
            return RedirectToAction("Profile", "Customer");
        }
        #endregion

        #region CustomerPostpage
        //method to load data for post page
        private void loadDataForPost(String search, int category, int startDate, int endDate, String sortBy, String order)
        {
            var postList = _context.Posts.Include(p => p.Categories)
                           .Where(p => p.Date > startDate && p.Date < endDate
                           && (p.Title.ToLower().Contains(search.ToLower())
                           | p.Content.ToLower().Contains(search.ToLower()))
                           && p.Status.StatusName == "Active")
                           .Select(p => new
                           {
                               ID = p.PostId,
                               Title = p.Title,
                               Categories = p.Categories,
                               Date = CommonMethod.intDateToStringDate(p.Date),
                               IntDate = p.Date,
                               View = p.ViewCount,
                               Content = p.Content.Substring(0, Math.Min(p.Content.Length, 100)) + "...",
                               ImageUrl = _context.Images
                                   .Where(i => i.ObjectId == p.PostId && i.TypeId == CommonMethod.getTypeID("Post"))
                                   .OrderBy(i => i.ImageId)
                                   .Select(i => i.ImageUrl)
                                   .FirstOrDefault()
                           })
                           .ToList();
            //filter by category
            if (category > 0)
            {
                postList = postList.Where(p => p.Categories.Any(p => p.CategoryId == category)).ToList();
            }

            //sort
            switch (sortBy)
            {
                case "View":
                    postList = order == "ASC" ? postList.OrderBy(p => p.View).ToList()
                             : postList.OrderByDescending(p => p.View).ToList();
                    break;
                case "Date":
                    postList = order == "ASC" ? postList.OrderBy(p => p.IntDate).ToList()
                             : postList.OrderByDescending(p => p.IntDate).ToList();
                    break;
            }

            //get all category of post
            var categoryList = _context.Categories
                               .Where(c => c.Type.TypeName == "Post" && c.Status.StatusName == "Active")
                               .Select(c => new
                               {
                                   ID = c.CategoryId,
                                   Name = c.CategoryName
                               })
                               .ToList();

            //get all recent post
            var recentPost = _context.Posts
                .Where(p => p.Status.StatusName == "Active")
                .Select(p => new
                {
                    ImageUrl = _context.Images
                                   .Where(i => i.ObjectId == p.PostId && i.TypeId == CommonMethod.getTypeID("Post"))
                                   .OrderBy(i => i.ImageId)
                                   .Select(i => i.ImageUrl)
                                   .FirstOrDefault(),
                    Title = p.Title,
                    Content = p.Content,
                    ID = p.PostId,
                    Date = p.Date
                })
                .OrderByDescending(p => p.Date).Take(3).ToList();

            ViewBag.postList = postList;
            ViewBag.categoryList = categoryList;
            ViewBag.recentPost = recentPost;
        }

        [HttpGet]
        public IActionResult Post()
        {
            if (HttpContext.Session.GetString("UserRole") == "Customer"
                | HttpContext.Session.GetString("UserRole") == null)
            {
                ViewBag.ActiveTitle = "post";
                ViewBag.search = "";
                ViewBag.startDate = "2000-01-01";
                ViewBag.endDate = DateTime.Today.ToString("yyyy-MM-dd");
                ViewBag.orderby = "View";
                ViewBag.order = "ASC";
                loadDataForPost("", 0, 20000101, 20240310, "View", "ASC");

                return View();
            }
            return View("Error");
        }

        [HttpPost]
        public IActionResult Post(IFormCollection form)
        {
            ViewBag.ActiveTitle = "post";
            ViewBag.search = form["search"].ToString().Trim();
            ViewBag.startDate = form["startDate"].ToString();
            ViewBag.endDate = form["endDate"].ToString();
            ViewBag.orderby = form["orderby"].ToString();
            ViewBag.order = form["order"].ToString();
            ViewBag.category = int.Parse(form["category"].ToString());

            int startDate = form["startDate"].ToString() == "" ? 0 : int.Parse(form["startDate"].ToString().Replace("-", ""));
            int endDate = form["endDate"].ToString() == "" ? 99999999 : int.Parse(form["endDate"].ToString().Replace("-", ""));

            loadDataForPost(form["search"].ToString().Trim(), int.Parse(form["category"].ToString()),
                            startDate, endDate,
                            form["orderby"].ToString(), form["order"].ToString());

            return View();
        }
        #endregion

        #region CustomerOrder
        [HttpGet]
        public IActionResult Order()
        {
            ViewBag.ActiveTitle = "order";
            var userID = HttpContext.Session.GetInt32("UserID");
            if (userID == null | HttpContext.Session.GetString("UserRole") != "Customer")
            {
                return View("Error");
            }
            else
            {
                ViewBag.startDate = "2000-01-01";
                ViewBag.endDate = DateTime.Now.ToString("yyyy-MM-dd");
                ViewBag.status = 0;

                var orderList = _context.Orders.Where(o => o.UserId == userID)
                .Select(o => new
                {
                    ID = o.OrderId,
                    OrderDate = CommonMethod.intDateToStringDate(o.OrderDate),
                    StatusName = o.Status.StatusName,
                    OrderDateInt = o.OrderDate

                }).OrderByDescending(o => o.OrderDateInt).ThenByDescending(o => o.ID).ToList();
                var statusList = _context.Statuses.Where(s => s.Type.TypeName == "Order")
                    .Select(s => new
                    {
                        ID = s.StatusId,
                        Name = s.StatusName
                    }).ToList();

                ViewBag.orderList = orderList;
                ViewBag.statusList = statusList;
            }
            TempData["date-validate"] = "Start date must be smaller than end date";
            return View();
        }

        [HttpPost]
        public IActionResult Order(IFormCollection form)
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            int startDate = form["startDate"].ToString() == "" ? 20000101
                : int.Parse(form["startDate"].ToString().Replace("-", ""));
            int endDate = form["endDate"].ToString() == "" ? CommonMethod.GetCurrentDateAsInt()
                : int.Parse(form["endDate"].ToString().Replace("-", ""));
            int statusID = int.Parse(form["status"].ToString());
            Console.WriteLine(startDate + ", " + endDate);
            ViewBag.status = statusID;
            ViewBag.startDate = startDate == 20000101 ? "2000-01-01" : form["startDate"].ToString();
            ViewBag.endDate = endDate == CommonMethod.GetCurrentDateAsInt() ? DateTime.Now.ToString("yyyy-MM-dd")
                : form["endDate"].ToString();

            var orderList = _context.Orders.Where(o => o.UserId == userID
            && (o.OrderDate > startDate && o.OrderDate <= endDate))
                .Select(o => new
                {
                    ID = o.OrderId,
                    OrderDate = CommonMethod.intDateToStringDate(o.OrderDate),
                    StatusName = o.Status.StatusName,
                    StatusID = o.StatusId

                }).ToList();
            if (statusID > 0)
            {
                orderList = orderList.Where(o => o.StatusID == statusID).ToList();
            }

            var statusList = _context.Statuses.Where(s => s.Type.TypeName == "Order")
                    .Select(s => new
                    {
                        ID = s.StatusId,
                        Name = s.StatusName
                    }).ToList();
            ViewBag.statusList = statusList;
            ViewBag.orderList = orderList;
            TempData["date-validate"] = "Start date must be smaller than end date";

            return View();
        }
        #endregion

        #region CustomerOrderDetail
        public async Task<IActionResult> OrderDetail(int id)
        {
            ViewBag.ActiveTitle = "order";
            var order = _context.Orders
                .Where(o => o.OrderId == id)
                .Select(o => new
                {
                    Note = o.OrderNote,
                    ID = o.OrderId,
                    OrderDate = DateTime.ParseExact(o.OrderDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                    ShipDate = o.ShipDate == 0 ? "" :
                    DateTime.ParseExact(o.ShipDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"),
                    ReceiverName = o.ReceiverName,
                    Phone = o.PhoneNumber,
                    ShippingUnit = o.ShippingUnit.Name,
                    AddressFrom = o.FromAddress.City + ", " + o.FromAddress.District + ", " + o.FromAddress.Street + ", " + o.FromAddress.Detail,
                    AddressTo = o.ToAddress.City + ", " + o.ToAddress.District + ", " + o.ToAddress.Street + ", " + o.ToAddress.Detail,
                    Payment = o.PaymentMethod,
                    Url = o.ShippingUnit.Website,
                    PaymentMethod = o.PaymentMethod,
                    StatusName = o.Status.StatusName
                }).FirstOrDefault();
            if (order == null | HttpContext.Session.GetString("UserRole") != "Customer")
            {
                return View("Error");
            }
            var detail = _context.OrderDetails
                .Where(d => d.OrderId == id)
                .Select(d => new
                {
                    Image = CommonMethod.getFirstImage((int)d.PaintingId),
                    Name = d.Painting.Name,
                    Quantity = d.Quantity,
                    Discount = d.Discount,
                    Price = CommonMethod.getActualPrice(d.Price, 1, 0)
                    .ToString("#,0", CultureInfo.InvariantCulture),
                    SumPrice = CommonMethod.getActualPrice(d.Price, d.Quantity, 0)
                    .ToString("#,0", CultureInfo.InvariantCulture),
                    DiscountPrice = CommonMethod.getActualPrice(d.Price, d.Quantity, d.Discount)
                    .ToString("#,0", CultureInfo.InvariantCulture)
                }).ToList();

            ViewBag.orderDetail = detail;
            ViewBag.order = order;
            ViewBag.totalPrice = await CommonMethod.calculateOrderPrice(id);
            return View();

        }

        //action to cancel an order
        [HttpPost]
        public IActionResult CancelOrder(IFormCollection form)
        {
            Console.WriteLine("vao form cancel?");
            //change status of order
            int orderID = int.Parse(form["id"].ToString());
            var order = _context.Orders.Include(t => t.OrderDetails)
                .Where(o => o.OrderId == orderID)
                .FirstOrDefault();
            order.StatusId = CommonMethod.getStatusID("Order", "Canceled");

            //var details = _context.OrderDetails
            //    .Where(od => od.OrderId==orderID)
            //    .ToList();
            foreach (OrderDetail detail in order.OrderDetails)
            {
                //return the painting quantity
                var painting = _context.Paintings
                    .Where(p => p.PaintingId == detail.PaintingId)
                    .FirstOrDefault();
                painting.Quantity += detail.Quantity;
            }

            _context.SaveChanges();
            return Redirect($"../OrderDetail?id={orderID}");
        }
        #endregion

        #region CustomerShoppingCart
        private void loadCartData()
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            if (userID != null)
            {
                var cartList = _context.Users.Include(t => t.Paintings).ThenInclude(t => t.Discount)
                    .Where(c =>
                    c.UserId == userID).Select(c => c.Paintings
                    ).FirstOrDefault();

                var cart = cartList.Select(c => new
                {
                    ID = c.PaintingId,
                    Quantity = c.Quantity,
                    Discount = c.Discount.Percentage,
                    OriginPrice = CommonMethod.getActualPrice(c.Price, 1, 0)
                    .ToString("#,0", CultureInfo.InvariantCulture),
                    Price = CommonMethod.getActualPrice(c.Price, 1, c.Discount.Percentage)
                    .ToString("#,0", CultureInfo.InvariantCulture),
                    Image = CommonMethod.getFirstImage(c.PaintingId),
                    Name = c.Name,
                    StatusName = _context.Paintings.Include(t => t.Status).Where(p => p.PaintingId == c.PaintingId)
                    .Select(p => p.Status.StatusName).FirstOrDefault()
                }).ToList();

                ViewBag.cart = cart;
            }
        }

        [HttpGet]
        public IActionResult Cart()
        {
            var userID = HttpContext.Session.GetInt32("UserID");
            if (userID == null | HttpContext.Session.GetString("UserRole") != "Customer")
            {
                return View("Error");
            }
            ViewBag.ActiveTitle = "cart";
            HttpContext.Session.Remove("paintingCheckout");
            HttpContext.Session.Remove("quantityCheckout");
            loadCartData();
            return View();
        }

        [HttpPost]
        public IActionResult Cart(IFormCollection form)
        {
            String[] id = form["id[]"].ToArray();
            String[] quantity = form["quantity[]"].ToArray();
            bool isEnoughtQuantity;
            for (int i = 0; i < id.Length; i++)
            {
                var painting = _context.Paintings.Where(p => p.PaintingId.ToString() == id[i]).FirstOrDefault();
                if (painting.Quantity < int.Parse(quantity[i]) | int.Parse(quantity[i])==0)
                {
                    ViewBag.alert = "One or more of the painting has change quantity. Please re-select your painting";
                    HttpContext.Session.Remove("paintingCheckout");
                    HttpContext.Session.Remove("quantityCheckout");
                    return Cart();
                }
            }

            //set session for checkout
            HttpContext.Session.SetComplexData("paintingCheckout", id);
            HttpContext.Session.SetComplexData("quantityCheckout", quantity);

            return Redirect("../Checkout");
        }

        [HttpPost]
        //method to remove cart item
        public IActionResult RemoveCart(IFormCollection form)
        {
            int userID = (int)HttpContext.Session.GetInt32("UserID");
            int paintingID = int.Parse(form["id"].ToString());
            // Get the user's cart (list of paintings)
            var cartList = _context.Users
                .Include(u => u.Paintings)
                .FirstOrDefault(u => u.UserId == userID)
                ?.Paintings;

            var user = _context.Users
            .Include(u => u.Paintings)
            .FirstOrDefault(u => u.UserId == userID);
            // Find the painting in the cart with the specified paintingID
            var paintingToRemove = user.Paintings.FirstOrDefault(p => p.PaintingId == paintingID);

            if (paintingToRemove != null)
            {
                // Remove the painting from the cart
                user.Paintings.Remove(paintingToRemove);

                // Save the changes to the database
                _context.SaveChanges();
                cartList = _context.Users.Include("Paintings").Where(c =>
                    c.UserId == userID).Select(c => c.Paintings).FirstOrDefault();
                int[] cartArray = cartList.Select(c => c.PaintingId).ToArray();
                HttpContext.Session.SetComplexData("cart", cartArray);
            }

            return RedirectToAction("Cart", "Customer");
        }
        #endregion

        #region CustomerCheckOut
        //method to get data for checkout
        private async Task loadDataCheckout(int addressid, int shippingunit)
        {
            String[] idArray = HttpContext.Session.GetComplexData<String[]>("paintingCheckout");
            String[] quantityArray = HttpContext.Session.GetComplexData<String[]>("quantityCheckout");

            bool isHeavy = false;
            bool isOutcity = false;
            bool isFragile = false;
            String orderType = "";
            List<object> detail = new List<object>();
            List<Voucher> vouchers = new List<Voucher>();
            List<int> voucherIDList = new List<int>();
            Dictionary<int, decimal> priceDict = new Dictionary<int, decimal>();
            int artistID = 0;

            int customerID = (int)HttpContext.Session.GetInt32("UserID");
            //get curent user's infor
            var currentUser = _context.Users
                .Where(u => u.UserId == customerID)
                .Select(u => new
                {
                    ID = u.UserId,
                    Name = u.UserName,
                    Phone = u.PhoneNumber,
                    Address = _context.Addresses.Include(t => t.StatusNavigation).Where(a => a.ObjectId == u.UserId
                    && a.Type.TypeName == "User")
                    .OrderBy(a => a.StatusNavigation.StatusId).ToList()
                }).FirstOrDefault();

            var defaultAddress = addressid == 0 ?
                currentUser.Address.Where(a => a.StatusNavigation.StatusName == "Default").FirstOrDefault() :
                _context.Addresses.Where(a => a.AddressId == addressid).FirstOrDefault();

            for (int i = 0; i < idArray.Length; i++)
            {
                var painting = _context.Paintings.Include(t => t.Discount)
                    .Where(p => p.PaintingId.ToString() == idArray[i])
                    .Select(p => new
                    {
                        ID = p.PaintingId,
                        UserID = p.UserId,
                        Name = p.Name,
                        Discount = p.Discount.Percentage,
                        ShopName = p.User.ShopName,
                        Quantity = int.Parse(quantityArray[i]),
                        Price = CommonMethod.getActualPrice(p.Price, int.Parse(quantityArray[i]), p.Discount.Percentage),
                        Height = p.Height,
                        Width = p.Width,
                        City = _context.Addresses.Where(a => a.Type.TypeName == "User"
                        && a.ObjectId == p.UserId).FirstOrDefault().City,
                        IsFragile = p.IsFragile
                    })
                    .FirstOrDefault();

                detail.Add(painting);
            }
            detail = detail.OrderBy(d => ((dynamic)d).UserID).ToList();
            detail.Add(new
            {
                ID = 0,
                UserID = 0,
                Name = "",
                Discount = 0,
                ShopName = "",
                Quantity = 0,
                Price = 0
            });

            //get voucher list
            dynamic firstElement = detail.FirstOrDefault();
            int FirstUserID = firstElement?.UserID;
            decimal totalShopPainting = 0;
            for (int i = 0; i < detail.Count - 1; i++)
            {
                dynamic item = detail[i];
                dynamic itemNext = detail[i + 1];
                //get voucher
                if ((int)itemNext.UserID == FirstUserID)
                {
                    totalShopPainting += (int)item.Price;
                }
                else
                {
                    int userID = (int)HttpContext.Session.GetInt32("UserID");
                    var voucher = _context.Vouchers
                        .Where(v => v.StartDate <= CommonMethod.GetCurrentDateAsInt()
                        && v.EndDate > CommonMethod.GetCurrentDateAsInt()
                        && v.UserId == FirstUserID
                        && _context.OrderVouchers
                        .Where(o => o.UserId == userID && o.VoucherId == v.VoucherId)
                        .FirstOrDefault() == null
                        && v.Status.StatusName == "Active")
                        .ToList();

                    if (voucher != null)
                    {
                        vouchers.AddRange(voucher);
                        for (int j = 0; j < voucher.Count; j++)
                        {
                            if (!voucherIDList.Contains((int)voucher[j].UserId))
                            {
                                voucherIDList.Add((int)voucher[j].UserId);
                            }
                        }

                    }
                    totalShopPainting = 0;
                    FirstUserID = (int)itemNext.UserID;
                }

                //check for heavy, fragile, in-out city standard to get shipping price
                if ((bool)item.IsFragile)
                {
                    isFragile = true;
                }
                if (!isHeavy)
                {
                    decimal? weight = (item.Height * item.Width) / 5000;
                    if (((item.Height + item.Width) > 200)
                    | weight >= 80
                    | (item.Height > 120 | item.Width > 120))
                    {
                        isHeavy = true;
                    }
                }
                if (!isOutcity)
                {
                    String City = defaultAddress.City;
                    if (City != item.City)
                    {
                        isOutcity = true;
                    }
                }


                if ((int)item.UserID != (int)itemNext.UserID)
                {
                    //get shipping price
                    if (!isOutcity)
                    {
                        orderType += "Incity";
                    }
                    else
                    {
                        orderType += "Outcity";
                    }
                    if (!isHeavy)
                    {
                        orderType += "-normal";
                    }
                    else
                    {
                        orderType += "-heavy";
                    }
                    if (isFragile)
                    {
                        orderType += "-fragile";
                    }
                    //get shipping unit information
                    decimal KM = 1;
                    if (isOutcity)
                    {
                        int aID = (int)item.UserID;
                        var artistAddress = _context.Addresses.Include(t => t.StatusNavigation)
                            .Where(a => a.ObjectId == aID && a.Type.TypeName == "User"
                            && a.StatusNavigation.StatusName == "Default")
                            .Select(a => new
                            {
                                Address = a.Detail + "," + a.Street + "," + a.District + "," + a.City
                            }).FirstOrDefault();
                        var userAddress = defaultAddress.Detail + "," + defaultAddress.Street + "," + defaultAddress.District + "," + defaultAddress.City;

                        String distance = await CommonMethod.GoogleMapDistance(
                            Regex.Replace(artistAddress.Address, @"\s+", ""),
                            Regex.Replace(userAddress, @"\s+", ""));
                        KM = decimal.Parse(distance) / 1000;
                    }
                    var perKMPrice = CommonMethod.getCurrentCoopPrice(shippingunit, "Outcity-perKM");
                    decimal? shippingPrice = CommonMethod.getCurrentCoopPrice(shippingunit, orderType).Price +
                        (perKMPrice.Price * ((int)KM / perKMPrice.PerKm));
                    priceDict.Add((int)item.UserID, (decimal)shippingPrice);
                    isHeavy = false;
                    isOutcity = false;
                    isFragile = false;
                    orderType = "";
                }
            }

            var shippingUnit = _context.ShippingUnits
                .Where(s => s.Status.StatusName == "Active")
                .Select(s => new
                {
                    ID = s.ShippingUnitId,
                    Name = s.Name
                })
                .OrderBy(s => s.ID).ToList();

            ViewBag.addressID = defaultAddress.AddressId;
            ViewBag.address = defaultAddress.Detail + ", " + defaultAddress.Street + ", " + defaultAddress.District + ", " + defaultAddress.City;
            ViewBag.user = currentUser;
            ViewBag.coop = shippingUnit;
            ViewBag.detail = detail;
            ViewBag.vouchers = vouchers;
            ViewBag.firstUserID = FirstUserID;
            ViewBag.voucherIDList = voucherIDList;
            ViewBag.priceDict = priceDict;
            ViewBag.coopID = shippingunit;
        }

        public async Task<IActionResult> CheckOut(IFormCollection form)
        {
            if (HttpContext.Session.GetString("UserRole") != "Customer")
            {
                return View("Error");
            }
            int addressid = form["addressid"].ToString() == "" ? 0
                : int.Parse(form["addressid"].ToString());
            int shippingunitid = form["shippingunitid"].ToString() == "" ? _context.ShippingUnits
                .Where(s => s.Status.StatusName == "Active")
                .OrderBy(s => s.ShippingUnitId)
                .FirstOrDefault().ShippingUnitId :
            int.Parse(form["shippingunitid"].ToString());

            await loadDataCheckout(addressid, shippingunitid);

            return View();
        }

        [HttpPost]
        //method to add a new address
        public IActionResult AddAddress(IFormCollection form)
        {
            String city = form["userCity"].ToString().Trim();
            String district = form["userDistrict"].ToString().Trim();
            String ward = form["userWards"].ToString().Trim();
            String detail = form["detail"].ToString().Trim();

            _context.Addresses.Add(new Address
            {
                City = city,
                District = district,
                Street = ward,
                Detail = detail,
                Status = CommonMethod.getStatusID("Address", "Active"),
                ObjectId = HttpContext.Session.GetInt32("UserID"),
                TypeId = CommonMethod.getTypeID("User")
            });
            _context.SaveChanges();

            return Redirect("../CheckOut");
        }

        #endregion

        #region CustomerOrderPayment
        [HttpPost]
        public IActionResult processingOrderPayment(IFormCollection form)
        {
            int? userID = HttpContext.Session.GetInt32("UserID");
            String[] idArray = HttpContext.Session.GetComplexData<String[]>("paintingCheckout");
            String[] quantityArray = HttpContext.Session.GetComplexData<String[]>("quantityCheckout");
            if (userID == null | idArray.Length == 0 | quantityArray.Length == 0)
            {
                return View("Error");
            }
            //get the order's infor
            var orderNote = form["orderNote"].ToString().Trim();
            var customerName = form["custName"].ToString().Trim();
            var customerPhone = form["phone"].ToString().Trim();
            var paymentMethod = "COD";
            int shippingunitid = int.Parse(form["coop"].ToString().Trim());
            int addressid = int.Parse(form["addressid"].ToString());
            String[] vouchers = form["voucher[]"].ToArray();

            //set key, value as pid, quantity
            Dictionary<int, int> quantityDict = new Dictionary<int, int>();
            for (int i = 0; i < idArray.Length; i++)
            {
                quantityDict.Add(int.Parse(idArray[i]), int.Parse(quantityArray[i]));
            }

            //get all the painting detail
            var detail = _context.Paintings.Include(t => t.Discount).Include(t => t.Status)
                .Where(p => idArray.Contains(p.PaintingId.ToString()))
                .Select(p => new
                {
                    ID = p.PaintingId,
                    UserID = p.UserId,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    BuyQuantity = quantityDict[p.PaintingId],
                    Discount = p.Discount.Percentage,
                    StatusName = p.Status.StatusName,
                    ArtistStatus = _context.Accounts.Where(a => a.AccountId==p.UserId)
                    .Select(a => a.Status.StatusName).FirstOrDefault(),
                    PaintingName = p.Name
                }).OrderBy(u => u.UserID).ToList();
            detail.Add(new
            {
                ID = 0,
                UserID = (int?)0,
                Price = (decimal?)0,
                Quantity = (int?)0,
                BuyQuantity = 0,
                Discount = (int?)0,
                StatusName = (string?)"",
                ArtistStatus = (string?)"",
                PaintingName = (string?)""
            });
            quantityDict.Clear();

            //create order by adding order and order detail
            List<OrderDetail> detailList = new List<OrderDetail>();
            for (int i = 0; i < detail.Count - 1; i++)
            {


                if (detail[i].Quantity < detail[i].BuyQuantity)
                {
                    ViewBag.alert("Quantity of a painting has been changed. Please re-select quantity.");
                    HttpContext.Session.Remove("paintingCheckout");
                    HttpContext.Session.Remove("quantityCheckout");
                    return Cart();
                }
                else if (detail[i].StatusName!="Active")
                {
                    ViewBag.alert("One of a painting's status has been changed. Please re-select painting");
                    HttpContext.Session.Remove("paintingCheckout");
                    HttpContext.Session.Remove("quantityCheckout");
                    return Cart();
                }
                else if (detail[i].ArtistStatus=="Disable")
                {
                    ViewBag.alert($"Artist of {detail[i].PaintingName} has been blocked. Please re-select painting");
                    HttpContext.Session.Remove("paintingCheckout");
                    HttpContext.Session.Remove("quantityCheckout");
                    return Cart();
                }
                else
                {
                    var item = detail[i];
                    var nextItem = detail[i + 1];

                    //create order detail
                    OrderDetail od = new OrderDetail
                    {
                        Quantity = item.BuyQuantity,
                        StatusId = CommonMethod.getStatusID("OrderDetail", "Processing"),
                        Discount = item.Discount,
                        PaintingId = item.ID,
                        Price = item.Price,
                        ReadyDate = 0
                    };
                    detailList.Add(od);

                    //create and add order
                    if (item.UserID != nextItem.UserID)
                    {
                        var date = DateTime.Now;
                        int fromAddresID = _context.Addresses
                            .Where(a => a.ObjectId == item.UserID
                            && a.Type.TypeName == "User"
                            && a.StatusNavigation.StatusName == "Default")
                            .Select(a => a.AddressId).FirstOrDefault();
                        String orderCode = "";
                        do
                        {
                            orderCode = CommonMethod.GeneratePassword(10);
                        } while (_context.Orders.Where(o => o.OrderCode == orderCode).FirstOrDefault() != null);

                        Order order = new Order
                        {
                            OrderNote = orderNote,
                            OrderDate = int.Parse(date.Year + "" + date.Month.ToString("D2") + ""
                            + date.Date.Day.ToString("D2")),
                            ShipDate = int.Parse(date.AddDays(5).Year + "" + date.AddDays(5).Month.ToString("D2") + ""
                            + date.AddDays(5).Date.Day.ToString("D2")),
                            ReceiverName = customerName,
                            PhoneNumber = customerPhone,
                            PaymentMethod = paymentMethod,
                            UserId = HttpContext.Session.GetInt32("UserID"),
                            ShippingUnitId = shippingunitid,
                            StatusId = CommonMethod.getStatusID("Order", "Processing"),
                            FromAddressId = fromAddresID,
                            ToAddressId = addressid,
                            OrderCode = orderCode

                        };
                        _context.Orders.Add(order);
                        _context.SaveChanges();

                        //adding voucher for order
                        if (vouchers.Length > 0)
                        {
                            for (int j = 0; j < vouchers.Length; j++)
                            {
                                int id = int.Parse(vouchers[j].Split(",")[1]);
                                var voucher = _context.Vouchers
                                    .Where(v => v.VoucherId == id).FirstOrDefault();
                                if (id != 0)
                                {
                                    if (voucher.UserId == item.UserID)
                                    {
                                        _context.OrderVouchers.Add(new OrderVoucher
                                        {
                                            UserId = (int)HttpContext.Session.GetInt32("UserID"),
                                            OrderId = order.OrderId,
                                            VoucherId = voucher.VoucherId
                                        });
                                        _context.SaveChanges();
                                    }
                                }
                            }
                        }

                        //add order detail
                        foreach (OrderDetail orderDetail in detailList)
                        {
                            orderDetail.OrderId = order.OrderId;
                        }
                        _context.OrderDetails.AddRange(detailList);
                        _context.SaveChanges();
                        detailList.Clear();


                    }
                }
            }

            //subtract all paining's quantity
            for (int i = 0; i < idArray.Length; i++)
            {
                var painting = _context.Paintings
                    .Where(p => p.PaintingId == int.Parse(idArray[i]))
                    .FirstOrDefault();
                painting.Quantity -= int.Parse(quantityArray[i]);
                _context.SaveChanges();
            }

            HttpContext.Session.Remove("paintingCheckout");
            HttpContext.Session.Remove("quantityCheckout");
            return RedirectToAction("Order", "Customer");
        }

        #endregion

        #region Post Detail

        private void loadPostDetailData(int id)
        {
            //get post data
            var post = _context.Posts.Include(t => t.Categories)
                .Where(p => p.PostId == id)
                .Select(p => new
                {
                    ID = p.PostId,
                    Title = p.Title,
                    Date = CommonMethod.intDateToStringDate(p.Date),
                    Content = p.Content,
                    Image = _context.Images
                        .Where(i => i.ObjectId == id && i.Type.TypeName == "Post")
                        .Select(i => new
                        {
                            ID = i.ImageId,
                            Url = i.ImageUrl
                        }).ToList(),
                    AuthorID = p.UserId,
                    Categories = p.Categories.Select(c => c.CategoryId).ToList()
                }).FirstOrDefault();

            //get author data
            var author = _context.Users
                .Where(u => u.UserId == post.AuthorID)
                .Select(u => new
                {
                    ID = u.UserId,
                    Name = u.UserName,
                    Email = u.Account.Email,
                    Phone = u.PhoneNumber,
                    Avatar = _context.Images
                    .Where(i => i.ObjectId == u.UserId && i.Type.TypeName == "User")
                    .Select(i => i.ImageUrl).FirstOrDefault()
                }).FirstOrDefault();

            //get categories
            var categories = _context.Categories
                .Where(c => c.Type.TypeName == "Post" && c.Status.StatusName == "Active")
                .Select(c => new
                {
                    ID = c.CategoryId,
                    Name = c.CategoryName
                }).ToList();

            //get recent post
            var recentPost = _context.Posts
                .Where(p => p.Status.StatusName == "Active")
                .Select(p => new
                {
                    ImageUrl = _context.Images
                                   .Where(i => i.ObjectId == p.PostId && i.TypeId == CommonMethod.getTypeID("Post"))
                                   .OrderBy(i => i.ImageId)
                                   .Select(i => i.ImageUrl)
                                   .FirstOrDefault(),
                    Title = p.Title,
                    Content = p.Content.Length > 100 ? p.Content.Substring(0, 100) + "..." : p.Content,
                    ID = p.PostId,
                    Date = p.Date,
                    Avatar = _context.Images
                    .Where(i => i.ObjectId == p.UserId && i.Type.TypeName == "User")
                    .Select(i => i.ImageUrl).FirstOrDefault()
                })
                .OrderByDescending(p => p.Date).Take(3).ToList();

            //get comment roor data
            var commentRoot = (from c in _context.Comments
                               join u in _context.Users on c.UserId equals u.UserId
                               join i in _context.Images on u.UserId equals i.ObjectId
                               join t1 in _context.Types on i.TypeId equals t1.TypeId
                               join t2 in _context.Types on c.TypeId equals t2.TypeId
                               where t1.TypeName == "User" && t2.TypeName == "Post"
                                     && c.CommentRepId == null && c.ObjectId == id
                               orderby c.CommentDate
                               select new
                               {
                                   ID = c.CommentId,
                                   Date = c.CommentDate,
                                   Content = c.Content,
                                   UserID = u.UserId,
                                   UserName = u.UserName,
                                   Avatar = i.ImageUrl
                               }).ToList();

            //get comment rep data
            List<List<Tuple<int, DateTime?, string, int, string, string, int?>>> commentRepsList =
            new List<List<Tuple<int, DateTime?, string, int, string, string, int?>>>();
            if (commentRoot.Count() > 0)
            {
                for (int index = 0; index < commentRoot.Count(); index++)
                {
                    var comment = commentRoot[index];
                    var commentRep = from c in _context.Comments
                                     join u in _context.Users on c.UserId equals u.UserId
                                     join i in _context.Images on u.UserId equals i.ObjectId
                                     join t1 in _context.Types on i.TypeId equals t1.TypeId
                                     join t2 in _context.Types on c.TypeId equals t2.TypeId
                                     where t1.TypeName == "User" && t2.TypeName == "Post"
                                           && c.CommentRepId == comment.ID && c.ObjectId == id
                                     orderby c.CommentDate
                                     select Tuple.Create(
                                         c.CommentId,
                                         c.CommentDate,
                                         c.Content,
                                         u.UserId,
                                         u.UserName,
                                         i.ImageUrl,
                                         c.CommentRepId
                                     );
                    // Create a new list of comment rep for each comment root
                    List<Tuple<int, DateTime?, string, int, string, string, int?>> commentReps =
                        new List<Tuple<int, DateTime?, string, int, string, string, int?>>();

                    // Add the comment representations to the new list
                    commentReps.AddRange(commentRep);

                    // Add the new list to the list of lists
                    commentRepsList.Add(commentReps);
                }
            }

            ViewBag.post = post;
            ViewBag.author = author;
            ViewBag.categories = categories;
            ViewBag.recentPost = recentPost;
            ViewBag.commentRoot = commentRoot;
            ViewBag.commentRep = commentRepsList;
            ViewBag.objectID = id;
        }


        public IActionResult PostDetail(int id)
        {
            if (HttpContext.Session.GetString("UserRole") == null |
                HttpContext.Session.GetString("UserRole") == "Customer")
            {
                loadPostDetailData(id);
                return View();
            }
            return View("Error");
        }

        //method to perform post comment action
        [HttpPost]
        public IActionResult CommentAction(IFormCollection form)
        {
            int userID = (int)HttpContext.Session.GetInt32("UserID");
            String action = form["action"].ToString().Trim();
            int objectID = form["objectID"].ToString() == "" ? 0 : int.Parse(form["objectID"].ToString());
            String content = form["content"].ToString().Trim();
            ViewBag.commentID = 0;

            //comment
            if (action == "comment")
            {
                Comment comment = new Comment
                {
                    Content = content,
                    CommentDate = DateTime.Now,
                    ObjectId = objectID,
                    TypeId = CommonMethod.getTypeID("Post"),
                    UserId = userID
                };
                _context.Comments.Add(comment);
                _context.SaveChanges();
                HttpContext.Session.SetString("commentPostID", comment.CommentId.ToString());
            }
            //rep comment
            else if (action == "rep")
            {
                int commentRepID = int.Parse(form["commentRepID"].ToString());
                Comment commentRep = new Comment
                {
                    Content = content,
                    CommentDate = DateTime.Now,
                    ObjectId = objectID,
                    TypeId = CommonMethod.getTypeID("Post"),
                    UserId = userID,
                    CommentRepId = commentRepID
                };
                _context.Comments.Add(commentRep);
                _context.SaveChanges();
                HttpContext.Session.SetString("commentPostID", commentRep.CommentId.ToString());
            }
            //delete comment
            else if (action == "delete")
            {
                int commentID = int.Parse(form["commentID"].ToString());
                var comment = _context.Comments.Where(c => c.CommentId == commentID).FirstOrDefault();
                comment.ObjectId = 0;
                _context.SaveChanges();
                var firstComment = _context.Comments
                                   .Where(c => c.Type.TypeName == "Post"
                                   && c.ObjectId == objectID && c.CommentRepId == null)
                                   .OrderBy(c => c.CommentDate)
                                   .FirstOrDefault();
                //check if this painting has any comment left
                if (firstComment == null)
                {
                    HttpContext.Session.SetString("commentPostID", "commentForm");
                }
                else
                {
                    HttpContext.Session.SetString("commentPostID", firstComment.CommentId.ToString());
                }
            }

            return Redirect("../PostDetail?id=" + objectID);
        }
        #endregion
    }
}




