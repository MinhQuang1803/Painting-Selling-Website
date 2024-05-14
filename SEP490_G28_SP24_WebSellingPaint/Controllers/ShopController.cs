using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using SEP490_G28_SP24_WebSellingPaint.FeatureCode;
using SEP490_G28_SP24_WebSellingPaint.FunctionCode;
using SEP490_G28_SP24_WebSellingPaint.Models;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace SEP490_G28_SP24_WebSellingPaint.Controllers
{
    public class ShopController : Controller
    {
        private readonly WebSellingPaintingContext _context;
        private readonly HttpClient client;
        private string googleMapApi = "https://maps.googleapis.com/maps/api/js?libraries=places&language=en&key=AIzaSyA-5tbF_zuVYg9ilGegwkED-3rqA7O_48w";

        public ShopController(WebSellingPaintingContext context)
        {
            this._context = context;
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
        }

        #region Shop page
        public IActionResult Shop()
        {
            if (HttpContext.Session.GetString("UserRole") == null
                | HttpContext.Session.GetString("UserRole") == "Customer")
            {
                ViewBag.ActiveTitle = "shop";
                ViewBag.searchbox = "";
                ViewBag.category = 0;
                ViewBag.minPrice = 0;
                ViewBag.maxPrice = 999999999;
                ViewBag.minWidth = 0;
                ViewBag.maxWidth = 9999;
                ViewBag.minHeight = 0;
                ViewBag.maxHeight = 9999;
                ViewBag.minDis = 0;
                ViewBag.maxDis = 99;
                ViewBag.orderby = "Date";
                ViewBag.order = "ASC";
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
                        .ToString("#,0", System.Globalization.CultureInfo.InvariantCulture),
                        DiscountPrice = CommonMethod.getActualPrice(g.Key.Price, 1, g.Key.Discount)
                        .ToString("#,0", System.Globalization.CultureInfo.InvariantCulture)
                    })
                    .OrderBy(p => p.Painting.ViewCount)
                .ToList();
                if (paintingsWithImages.ToList().Count == 0)
                {
                    ViewBag.alert = "There are no painting for now. Please come back later.";
                }
                else
                {
                    ViewBag.PaintingList = paintingsWithImages;
                }

                List<Category> categories = CommonMethod.GetActiveCategories("Painting", "Active");
                ViewBag.CategoryList = categories;
                List<Tuple<string, string>> orderbyList = new List<Tuple<string, string>>
            {
                Tuple.Create("Date", "Publish Date"),
                Tuple.Create("Price", "Price"),
                Tuple.Create("Discount", "Discount")
            };
                ViewBag.orderbyList = orderbyList;
                return View();
            }
            return View("Error");
        }


        [HttpPost]
        public IActionResult Shop(IFormCollection form)
        {
            Dict dict = new Dict
            {
                {"searchbox", form["searchbox"].ToString()},
                {"category", form["category"].ToString()},
                {"minPrice", form["minPrice"].ToString()},
                {"maxPrice", form["maxPrice"].ToString()},
                {"minWidth", form["minWidth"].ToString()},
                {"maxWidth", form["maxWidth"].ToString()},
                {"minHeight", form["minHeight"].ToString()},
                {"maxHeight", form["maxHeight"].ToString()},
                {"minDis", form["minDis"].ToString()},
                {"maxDis", form["maxDis"].ToString()},
                {"orderby", form["orderby"].ToString()},
                {"order", form["order"].ToString()}
            };
            return RedirectToAction("PostShop", dict);
        }

        public IActionResult PostShop(Dict form)
        {
            ViewBag.ActiveTitle = "shop";
            ViewBag.searchbox = form["searchbox"].ToString().Trim();
            ViewBag.category = int.Parse(form["category"].ToString().Trim());
            ViewBag.minPrice = int.Parse(form["minPrice"].ToString());
            ViewBag.maxPrice = int.Parse(form["maxPrice"].ToString());
            ViewBag.minWidth = int.Parse(form["minWidth"].ToString());
            ViewBag.maxWidth = int.Parse(form["maxWidth"].ToString());
            ViewBag.minHeight = int.Parse(form["minHeight"].ToString());
            ViewBag.maxHeight = int.Parse(form["maxHeight"].ToString());
            ViewBag.minDis = int.Parse(form["minDis"].ToString());
            ViewBag.maxDis = int.Parse(form["maxDis"].ToString());
            ViewBag.orderby = form["orderby"].ToString();
            ViewBag.order = form["order"].ToString();


            //filter by stats
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
                .Where(p => p.Painting.Name.Contains(form["searchbox"].ToString().Trim())
                && (p.Painting.Price - p.Painting.Price * p.Discount / 100 >= decimal.Parse(form["minPrice"].ToString().Trim())
                    && p.Painting.Price - p.Painting.Price * p.Discount / 100 < decimal.Parse(form["maxPrice"].ToString().Trim()))
                && (p.Painting.Width >= decimal.Parse(form["minWidth"].ToString().Trim())
                    && p.Painting.Width < decimal.Parse(form["maxWidth"].ToString().Trim()))
                && (p.Painting.Height >= decimal.Parse(form["minHeight"].ToString().Trim())
                    && p.Painting.Height < decimal.Parse(form["maxHeight"].ToString().Trim()))
                && (p.Discount >= int.Parse(form["minDis"].ToString().Trim())
                    && p.Discount < int.Parse(form["maxDis"].ToString().Trim()))
                )
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
                    .ToString("#,0", System.Globalization.CultureInfo.InvariantCulture),
                    DiscountPrice = CommonMethod.getActualPrice(g.Key.Price, 1, g.Key.Discount)
                    .ToString("#,0", System.Globalization.CultureInfo.InvariantCulture)
                })
            .ToList();
            //filter by category
            int cateID = int.Parse(form["category"].ToString());
            if (cateID != 0)
            {
                paintingsWithImages = paintingsWithImages
                .Where(p => CommonMethod.IsCategory(p.Painting.PaintingId, cateID)==true).ToList();
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
                    case "Date":
                        paintingsWithImages = form["order"].ToString() == "ASC" ?
                            paintingsWithImages.OrderBy(p => p.Painting.PublishDate).ToList() :
                            paintingsWithImages.OrderByDescending(p => p.Painting.PublishDate).ToList();
                        break;
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

            List<Category> categories = CommonMethod.GetActiveCategories("Painting", "Active");
            ViewBag.CategoryList = categories;
            List<Tuple<string, string>> orderbyList = new List<Tuple<string, string>>
            {
                Tuple.Create("Date", "Publish Date"),
                Tuple.Create("Price", "Price"),
                Tuple.Create("Discount", "Discount")
            };
            ViewBag.orderbyList = orderbyList;
            return View("Shop");
        }
        #endregion

        #region PaintingDetail page
        private void loadDataForPaintingDetail(int id)
        {
            //get painting data
            var painting = _context.Paintings.Where(p => p.PaintingId == id)
                           .Select(p => new
                           {
                               ID = p.PaintingId,
                               Name = p.Name,
                               Price = p.Price,
                               Discount = p.Discount.Percentage,
                               Description = p.Description,
                               Quantity = p.Quantity,
                               AuthorID = p.UserId,
                               Categories = p.Categories.ToList(),
                               View = p.ViewCount
                           })
                           .FirstOrDefault();


            //get image data
            var imageList = _context.Images.Where(i => i.Type.TypeName == "Painting" && i.ObjectId == id)
                                           .Select(i => new
                                           {
                                               ID = i.ImageId,
                                               Url = i.ImageUrl
                                           })
                                           .ToList();
            var imagePop = _context.Images.Where(i => i.Type.TypeName == "Painting" && i.ObjectId == id)
                                           .Select(i => new
                                           {
                                               ID = i.ImageId,
                                               Url = i.ImageUrl
                                           })
                                           .FirstOrDefault();
            //get author data
            var author = _context.Users
                .Where(u => u.UserId == painting.AuthorID)
                .Join(
                    _context.Images.Where(i => i.Type.TypeName == "User"),
                    user => user.UserId,
                    image => image.ObjectId,
                    (user, image) => new { user, image }
                )
                .SelectMany(ui =>
                    _context.Addresses.Include(s => s.StatusNavigation)
                        .Where(a => a.ObjectId == ui.user.UserId && a.StatusNavigation.StatusName == "Default")
                        .Select(a => new
                        {
                            UserId = ui.user.UserId,
                            UserName = ui.user.UserName,
                            PhoneNumber = ui.user.PhoneNumber,
                            Email = ui.user.Account.Email,
                            ArtistBackground = ui.user.ArtistBackground,
                            AccountId = ui.user.Account.AccountId,
                            ImageUrl = ui.image.ImageUrl,
                            Address = a.City + ", " + a.District + ", " + a.Street + ", " + a.Detail
                        })
                )
                .FirstOrDefault();

            //get related painting
            var categories = painting.Categories.Select(c =>
                c.CategoryId
            ).ToList();

            var related = (from p in _context.Paintings.Include(t => t.Status).Include(t => t.User)
                           from c in p.Categories
                           where c.Type.TypeName == "Painting" && categories.Contains(c.CategoryId)
                           && p.PaintingId != id && p.Status.StatusName=="Active"
                           select new
                           {
                               PaintingID = p.PaintingId,
                               CategoryID = c.CategoryId,
                               ImageUrl = CommonMethod.getFirstImage(p.PaintingId),
                               Discount = p.Discount.Percentage,
                               Price = p.Price,
                               Name = p.Name
                           })
                           .Take(4).OrderBy(p => p.PaintingID).GroupBy(p => p.PaintingID)
                           .Select(group => group.First()).ToList();
            ViewBag.related = related;

            //set default if the current painting dont have any related painting
            if (related.Count() == 0)
            {

                var related2 = _context.Paintings.Where(p => p.Status.StatusName == "Active")
                          .Select(p => new
                          {
                              PaintingID = p.PaintingId,
                              ImageUrl = CommonMethod.getFirstImage(p.PaintingId),
                              Discount = p.Discount.Percentage,
                              Price = p.Price,
                              Name = p.Name
                          }).ToList().Take(4);
                ViewBag.related = related2;
            }


            //get the comment data
            var commentRoot = (from c in _context.Comments
                               join u in _context.Users on c.UserId equals u.UserId
                               join i in _context.Images on u.UserId equals i.ObjectId
                               join t1 in _context.Types on i.TypeId equals t1.TypeId
                               join t2 in _context.Types on c.TypeId equals t2.TypeId
                               where t1.TypeName == "User" && t2.TypeName == "Painting"
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

            //get comment reply data follow order of comment list
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
                                     where t1.TypeName == "User" && t2.TypeName == "Painting"
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


            ViewBag.objectID = id;
            ViewBag.commentRoot = commentRoot;
            ViewBag.commentRep = commentRepsList;
            ViewBag.painting = painting;
            ViewBag.imageList = imageList;
            ViewBag.imagePop = imagePop;
            ViewBag.author = author;
        }


        [HttpGet]
        public IActionResult PaintingDetail(int id)
        {
            var painting = _context.Paintings
                            .Where(p => p.PaintingId == id && p.Status.StatusName == "Active")
                            .FirstOrDefault();
            if (painting!=null && (HttpContext.Session.GetString("UserRole") == null
                | HttpContext.Session.GetString("UserRole")=="Customer"))
            {
                painting.ViewCount+=1;
                _context.SaveChanges();
                loadDataForPaintingDetail(painting.PaintingId);
                return View();
            }
            return View("Error");
        }

        [HttpPost]
        public IActionResult PaintingDetail(IFormCollection form)
        {
            int userID = form["userID"].ToString() == "" ? 0 : int.Parse(form["userID"].ToString());
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
                    TypeId = CommonMethod.getTypeID("Painting"),
                    UserId = userID
                };
                _context.Comments.Add(comment);
                _context.SaveChanges();
                HttpContext.Session.SetString("commentID", comment.CommentId.ToString());
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
                    TypeId = CommonMethod.getTypeID("Painting"),
                    UserId = userID,
                    CommentRepId = commentRepID
                };
                _context.Comments.Add(commentRep);
                _context.SaveChanges();
                HttpContext.Session.SetString("commentID", commentRep.CommentId.ToString());
            }
            //delete comment
            else if (action == "delete")
            {
                int commentID = int.Parse(form["commentID"].ToString());
                var comment = _context.Comments.Where(c => c.CommentId == commentID).FirstOrDefault();
                comment.ObjectId = 0;
                _context.SaveChanges();
                var firstComment = _context.Comments
                                   .Where(c => c.Type.TypeName == "Painting"
                                   && c.ObjectId == objectID && c.CommentRepId == null)
                                   .OrderBy(c => c.CommentDate)
                                   .FirstOrDefault();
                //check if this painting has any comment left
                if (firstComment == null)
                {
                    HttpContext.Session.SetString("commentID", "commentForm");
                }
                else
                {
                    HttpContext.Session.SetString("commentID", firstComment.CommentId.ToString());
                }
            }
            //add painting to cart
            else if (action == "addCart")
            {
                int[] cart = HttpContext.Session.GetComplexData<int[]>("cart");
                if (!cart.Contains(objectID))
                {
                    // Get the user's cart (list of paintings)
                    var cartList = _context.Users
                        .Include(u => u.Paintings)
                        .FirstOrDefault(u => u.UserId == userID)
                        ?.Paintings;

                    // Get the painting to add to the cart
                    var paintingToAdd = _context.Paintings
                        .FirstOrDefault(p => p.PaintingId == objectID);

                    // Add the painting to the cart
                    if (cartList != null && paintingToAdd != null)
                    {
                        cartList.Add(paintingToAdd);
                        _context.SaveChanges();
                    }
                    cartList = _context.Users.Include("Paintings").Where(c =>
                    c.UserId==userID).Select(c => c.Paintings).FirstOrDefault();
                    int[] cartArray = cartList.Select(c => c.PaintingId).ToArray();
                    HttpContext.Session.SetComplexData("cart", cartArray);
                    ViewBag.success = "Painting has been added to cart";
                }
            }
            //loadDataForPaintingDetail(objectID);

            return Redirect("../PaintingDetail?id="+objectID);
        }

        //method to perform buy now function
        [HttpPost]
        public IActionResult BuyNow(IFormCollection form)
        {
            String[] id = { form["id"].ToString() };
            String[] quantity = { form["quantity"].ToString() };
            //set session for checkout
            HttpContext.Session.SetComplexData("paintingCheckout", id);
            HttpContext.Session.SetComplexData("quantityCheckout", quantity);
            ViewBag.goback = "Shop";
            return Redirect("../Checkout");
        }

        #endregion

    }
}
