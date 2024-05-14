using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SEP490_G28_SP24_WebSellingPaint.FunctionCode.GoogleResponseModel;
using SEP490_G28_SP24_WebSellingPaint.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SEP490_G28_SP24_WebSellingPaint.FunctionCode
{
    public static class CommonMethod
    {
        private static Random random = new Random();
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        // Method to encode the password
        public static string EncodePassword(string password)
        {
            byte[] data = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(data);
        }

        // Method to decode the encoded password
        public static string DecodePassword(string encodedPassword)
        {
            byte[] data = Convert.FromBase64String(encodedPassword);
            return Encoding.UTF8.GetString(data);
        }

        public static bool IsStrongPassword(string password)
        {
            // Check if the password is at least 8 characters long
            if (password.Length < 8)
            {
                return false;
            }

            // Check if the password contains at least one uppercase letter
            if (!password.Any(char.IsUpper))
            {
                return false;
            }

            return true;
        }

        public static bool IsVaildPhoneNumber(string phone)
        {
            if (phone == null)
            {
                return false;
            }
            // Regular expression pattern for validating email addresses
            string pattern = @"[0-9]{10}";

            // Use Regex.IsMatch method to check if the email matches the pattern
            return Regex.IsMatch(phone, pattern);
        }


        public static bool IsValidEmail(string email)
        {
            if (email == null)
            {
                return false;
            }
            // Regular expression pattern for validating email addresses
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            // Use Regex.IsMatch method to check if the email matches the pattern
            return Regex.IsMatch(email, pattern);
        }

        public static string GeneratePassword(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder sb = new StringBuilder(length);
            Random random = new Random();
            // Add at least 1 uppercase letter
            sb.Append(chars[random.Next(26, chars.Length)]);

            // Add remaining characters randomly
            for (int i = 1; i < length; i++)
            {
                sb.Append(chars[random.Next(chars.Length)]);
            }

            // Shuffle the characters
            for (int i = 0; i < length - 1; i++)
            {
                int j = random.Next(i, length);
                char temp = sb[i];
                sb[i] = sb[j];
                sb[j] = temp;
            }

            return sb.ToString();
        }

        //get the type id base on type name
        public static int getTypeID(string typeName)
        {
            using (WebSellingPaintingContext DBContext = new WebSellingPaintingContext())
            {
                var TypeID = DBContext.Types
                                     .Where(t => t.TypeName == typeName)
                                     .Select(t => t.TypeId)
                                     .FirstOrDefault();
                return TypeID;
            }
        }

        //get status id base on typename and status name
        public static int getStatusID(String typeName, String statusName)
        {
            using (WebSellingPaintingContext DBContext = new WebSellingPaintingContext())
            {
                var statusID = DBContext.Statuses
                                        .Where(st => st.StatusName == statusName
                                        && st.TypeId == getTypeID(typeName))
                                        .Select(st => st.StatusId)
                                        .FirstOrDefault();
                return statusID;
            }
        }

        //get the RoleID base on the role
        public static int getRoleID(String role)
        {
            using (WebSellingPaintingContext DBContext = new WebSellingPaintingContext())
            {
                var roleID = DBContext.Roles
                                      .Where(r => r.RoleName == role)
                                      .Select(r => r.RoleId)
                                      .FirstOrDefault();
                return roleID;
            }
        }

        //method to convert current date to a number
        public static int GetCurrentDateAsInt()
        {
            DateTime currentDate = DateTime.Now;
            int dateAsInt = currentDate.Year * 10000 + currentDate.Month * 100 + currentDate.Day;
            return dateAsInt;
        }

        //method to get all active painting category
        public static List<Category> GetActiveCategories(String TypeName, String StatusName)
        {
            using (WebSellingPaintingContext DBContext = new WebSellingPaintingContext())
            {
                var ActiveCategories = DBContext.Categories
                .Where(c => c.Type.TypeName == TypeName && c.Status.StatusName == StatusName).ToList();
                return ActiveCategories;
            }
        }

        //method to check if a painting belong to a category
        public static bool IsCategory(int pID, int cateID)
        {
            using (WebSellingPaintingContext DBContext = new WebSellingPaintingContext())
            {
                var painting = DBContext.Paintings
                    .Include(c => c.Categories)
                    .FirstOrDefault(p => p.PaintingId == pID);
                return painting.Categories.Any(c => c.CategoryId == cateID);
            }
        }

        //method to save image to a path
        public static void SaveBase64Image(string base64ImageString, string outputPath)
        {
            try
            {
                // Extract the image format and data from the base64 string
                string[] parts = base64ImageString.Split(',');
                string imageFormat = parts[0].Split('/')[1].Split(';')[0];
                string imageData = parts[1];

                // Convert the base64 data to a byte array
                byte[] bytes = Convert.FromBase64String(imageData);

                // Save the byte array to a file
                File.WriteAllBytes(outputPath, bytes);

                Console.WriteLine("Image saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving image: " + ex.Message);
            }
        }

        public static string GenerateRandomString(int length)
        {
            var stringChars = new char[length];
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }

        public static string ConvertUserId(int? id)
        {
            string convertUserID = Convert.ToString(id);
            return convertUserID;
        }



        //method to convert from DB INT Date to string Date
        public static String intDateToStringDate(int? intDate)
        {
            DateTime date = DateTime.ParseExact(intDate.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
            return date.ToString("dd/MM/yyyy");
        }

        //function to get first url image of a painting
        public static String getFirstImage(int id)
        {
            using (WebSellingPaintingContext DBContext = new WebSellingPaintingContext())
            {
                var url = DBContext.Images.Where(i => i.Type.TypeName == "Painting" && i.ObjectId == id)
                    .Select(i => i.ImageUrl).FirstOrDefault();
                return url;
            }
        }

        //method to calculate actual price of an order detail
        public static decimal getActualPrice(decimal? price, int? quantity, int? discount)
        {
            var actualPrice = Math.Ceiling(
            (decimal)((price ?? 0) * (quantity ?? 0) - (price ?? 0) * (discount ?? 0) / 100 * (quantity ?? 0)) / 1000) * 1000;
            return actualPrice;
        }

        //method to interact with session
        public static T GetComplexData<T>(this ISession session, string key)
        {
            var data = session.GetString(key);
            if (data == null)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(data);
        }

        //method to interact with session
        public static void SetComplexData(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        //method to show all painting categories in a string
        public static String getStringPaintingCate(int id)
        {
            String cate = "";
            using (WebSellingPaintingContext DBContext = new WebSellingPaintingContext())
            {
                var cates = DBContext.Paintings.Where(p => p.PaintingId == id)
                    .Select(p => p.Categories.Select(c => c.CategoryName).ToList()).FirstOrDefault();
                foreach (var item in cates)
                {
                    cate += $", {item}";
                }
                return cate.Substring(2);
            }
        }

        //method to calculate total price of an artist
        public static decimal getTotalPricePerWeeks(int year, int month, int userID, int startDate, int endDate)
        {
            using (WebSellingPaintingContext _context = new WebSellingPaintingContext())
            {
                //var totalSum = _context.OrderDetails
                //.Join(_context.Paintings, od => od.PaintingId, p => p.PaintingId, (od, p) => new { OrderDetail = od, Painting = p })
                //.Join(_context.Orders, od_p => od_p.OrderDetail.OrderId,
                //o => o.OrderId, (od_p, o) => new { od_p.OrderDetail, od_p.Painting, Order = o })
                //.Join(_context.Statuses, od_p_o => od_p_o.Order.StatusId, s => s.StatusId,
                //(od_p_o, s) => new { od_p_o.OrderDetail, od_p_o.Painting, od_p_o.Order, Status = s })
                //.Where(joined =>
                //joined.Painting.UserId == userID &&
                //(joined.OrderDetail.ReadyDate / 10000) == year &&
                //((joined.OrderDetail.ReadyDate / 100) % 100) == month &&
                //((joined.OrderDetail.ReadyDate % 100) >= startDate && (joined.OrderDetail.ReadyDate % 100) <= endDate) &&
                //joined.Status.StatusName == "Successfully")
                //.Sum(joined =>
                //(decimal)(joined.OrderDetail.Price * joined.OrderDetail.Quantity) -
                //((joined.OrderDetail.Price * joined.OrderDetail.Quantity * joined.OrderDetail.Discount) / 100));
                decimal totalSum = 0;
                var details = _context.OrderDetails
                    .Where(od => od.Painting.UserId==userID &&
                    (od.Order.ShipDate/10000==year && od.Order.ShipDate/100%100==month
                    && od.Order.ShipDate%100>=startDate && od.Order.ShipDate%100<=endDate)
                    && od.Order.Status.StatusName=="Successfully")
                    .Select(od => CommonMethod.getActualPrice(od.Price, od.Quantity, od.Discount))
                    .ToList();
                foreach (var item in details)
                {
                    totalSum+=item;
                }

                return totalSum;
            }
        }

        //method to calculate number order success/failed in month
        public static int getOrderPerMonth(int year, int month, int userID,
            int startDate, int endDate, String[] status)
        {
            using (WebSellingPaintingContext context = new WebSellingPaintingContext())
            {
                List<int> orders = new List<int>();
                //var orders = context.OrderDetails
                //.Where(od => od.Painting.UserId == userID && od.ReadyDate / 10000 == year)
                //.Join(context.Orders,
                //od => od.OrderId,
                //o => o.OrderId,
                //(od, o) => new { OrderDetail = od, Order = o })
                //.Where(x => x.Order.ShipDate / 100 % 100 == month && x.Order.ShipDate % 100 >= startDate && x.Order.ShipDate % 100 <= endDate)
                //.Join(context.Statuses,
                //x => x.Order.StatusId,
                //s => s.StatusId,
                //(x, s) => new { x.OrderDetail, x.Order, Status = s })
                //.Where(x => status.Contains(x.Status.StatusName)) 
                //.GroupBy(x => x.Order.OrderId)
                //.Select(g => g.Key)
                //.ToList();
                var ordersList = context.OrderDetails
                    .Where(od => od.Painting.UserId==userID && status.Contains(od.Order.Status.StatusName)
                    && (od.Order.ShipDate/10000==year && od.Order.ShipDate/100%100==month
                    && od.Order.ShipDate%100 >= startDate && od.Order.ShipDate%100<endDate)
                    && od.Order.Status.StatusName=="Successfully")
                    .ToList();
                foreach (var item in ordersList)
                {
                    if (!orders.Contains((int)item.OrderId))
                    {
                        orders.Add((int)item.OrderId);
                    }
                }

                return orders.Count();
            }
        }

        //method to get curent price base on type
        public static ShippingPrice getCurrentCoopPrice(int coopID, String priceType)
        {
            using (WebSellingPaintingContext _context = new WebSellingPaintingContext())
            {
                var price = _context.ShippingPrices
                .Where(sp => sp.ShippingUnitId == coopID && sp.Type.TypeName == priceType &&
                 (sp.StartDate / 100) <= DateTime.Now.Year
                 && (sp.StartDate % 100) <= DateTime.Now.Month)
                .OrderByDescending(sp => sp.StartDate)
                .FirstOrDefault();
                return price;
            }
        }

        //method to calculate distance for incity
        public static async Task<string> GoogleMapDistanceAvoidHighWay(string originAddress, string destinationAddress)
        {
            var origin = originAddress;
            var destination = destinationAddress;
            var distanceResult = "Null";
            var url = "https://maps.googleapis.com/maps/api/distancematrix/json?origins=" +
                origin + "&destinations=" + destination + "&avoid=highways&key=AIzaSyA-5tbF_zuVYg9ilGegwkED-3rqA7O_48w" +
                "&trafficmodel=OPTIMISTIC&departure_time=now";

            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            DistanceRespones data = JsonConvert.DeserializeObject<DistanceRespones>(responseString);
            if (data != null)
            {
                var divineDistance = (data.rows[0].elements[0].distance.value).ToString();
                distanceResult = divineDistance.ToString();

            }
            else
            {
                distanceResult = "Không có đường bộ giữa hai điểm";
            }

            return distanceResult;
        }

        public static async Task<string> GoogleMapDistance(string originAddress, string destinationAddress)
        {
            var origin = originAddress;
            var destination = destinationAddress;
            var distanceResult = "Null";
            var url = "https://maps.googleapis.com/maps/api/distancematrix/json?origins=" +
                origin + "&destinations=" + destination + "&key=AIzaSyA-5tbF_zuVYg9ilGegwkED-3rqA7O_48w" +
                "&trafficmodel=OPTIMISTIC&departure_time=now";

            var client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            var responseString = await response.Content.ReadAsStringAsync();
            DistanceRespones data = JsonConvert.DeserializeObject<DistanceRespones>(responseString);
            if (data != null)
            {
                var divineDistance = (data.rows[0].elements[0].distance.value).ToString();
                distanceResult = divineDistance.ToString();

            }
            else
            {
                distanceResult = "Không có đường bộ giữa hai điểm";
            }

            return distanceResult;
        }

        //method to get all coop's prices
        public static Dictionary<int, decimal?> getPriceList(int coopID, int year, String priceType)
        {
            using (WebSellingPaintingContext _context = new WebSellingPaintingContext())
            {
                var coop = _context.ShippingUnits.Include(t => t.ShippingPrices).ThenInclude(t => t.Type)
                    .Where(c => c.ShippingUnitId == coopID)
                    .Select(c => new
                    {
                        Prices = c.ShippingPrices
                    })
                    .FirstOrDefault();


                var priceDict = coop.Prices.Where(p => p.StartDate / 100 == year && p.Type.TypeName == priceType)
                .OrderBy(p => p.StartDate)
                .ToDictionary(p => (int)p.StartDate, p => p.Price);

                if (priceDict.Count == 0)
                {
                    var addedPrice = coop.Prices
                        .Where(p => p.Type.TypeName == priceType && p.StartDate / 100 < year)
                        .OrderByDescending(t => t.StartDate)
                        .Select(p => new
                        {
                            Date = p.StartDate,
                            Price = p.Price
                        }).FirstOrDefault();
                    priceDict.Add((int)addedPrice.Date, addedPrice.Price);
                }
                else if (priceDict.FirstOrDefault().Key % 100 > 1)
                {
                    var addedPrice = coop.Prices
                        .Where(p => p.Type.TypeName == priceType
                        && p.StartDate < priceDict.FirstOrDefault().Key)
                        .OrderByDescending(t => t.StartDate)
                        .Select(p => new
                        {
                            Date = p.StartDate,
                            Price = p.Price
                        }).FirstOrDefault();

                    if (addedPrice != null)
                    {
                        priceDict.Add(year * 100 + 1, addedPrice.Price);
                    }

                }
                priceDict = priceDict.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
                return priceDict;
            }
        }

        //method to calculate totel price of an order
        public static async Task<decimal> calculateOrderPrice(int orderID)
        {
            using (WebSellingPaintingContext DBContext = new WebSellingPaintingContext())
            {
                decimal totalPrice = 0;
                bool isHeavy = false;
                bool isOutcity = false;
                bool isFragile = false;
                String orderType = "";

                //get order and it's detail
                var order = DBContext.Orders
                    .Where(o => o.OrderId==orderID)
                    .Select(o => new
                    {
                        ID = o.OrderId,
                        From = Regex.Replace($"{o.FromAddress.Detail},{o.FromAddress.Street}," +
                        $"{o.FromAddress.District},{o.FromAddress.City}", @"\s+", ""),
                        To = Regex.Replace($"{o.ToAddress.Detail},{o.ToAddress.Street}," +
                        $"{o.ToAddress.District},{o.ToAddress.City}", @"\s+", ""),
                        CoopID = o.ShippingUnitId,
                        OrderDate = o.OrderDate
                    }).FirstOrDefault();

                var details = DBContext.OrderDetails
                    .Where(od => od.OrderId==orderID)
                    .Select(od => new
                    {
                        ID = od.PaintingId,
                        Price=CommonMethod.getActualPrice(od.Price, od.Quantity, od.Discount)
                    })
                    .ToList();

                //calculate price by order detail
                foreach (var item in details)
                {
                    totalPrice += item.Price;
                    var painting = DBContext.Paintings
                        .Where(p => p.PaintingId==item.ID)
                        .Select(p => new
                        {
                            Height = p.Height,
                            Width = p.Width,
                            IsFragile = (bool)p.IsFragile
                        })
                        .FirstOrDefault();
                    //check for heavy, fragile, in-out city standard to get shipping price
                    if (painting.IsFragile)
                    {
                        isFragile = true;
                    }
                    if (!isHeavy)
                    {
                        decimal? weight = (painting.Height * painting.Width) / 5000;
                        if (((painting.Height + painting.Width) > 200)
                        | weight >= 80
                        | (painting.Height > 120 | painting.Width > 120))
                        {
                            isHeavy = true;
                        }
                    }
                    if (!isOutcity)
                    {
                        if (order.From.Split(",")[3] != order.To.Split(",")[3])
                        {
                            isOutcity = true;
                        }
                    }
                }

                //calculate price by order type
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

                //adding total price
                var prices = DBContext.ShippingPrices
                    .Where(sp => sp.Type.TypeName==orderType && sp.ShippingUnitId==order.CoopID)
                    .Select(sp => new
                    {
                        ID = sp.PriceId,
                        Price = sp.Price,
                        StartDate = sp.StartDate
                    }).OrderByDescending(sp => sp.StartDate).ToList();

                foreach (var item in prices)
                {
                    if (item.StartDate/100<=order.OrderDate)
                    {
                        totalPrice += (decimal)item.Price;
                        break;
                    }
                }
                if (isOutcity)
                {
                    decimal KM = 1;
                    String distance = await CommonMethod.GoogleMapDistance(order.From, order.To);
                    KM = decimal.Parse(distance)/1000;

                    var perKMs = DBContext.ShippingPrices
                        .Where(sp => sp.Type.TypeName=="Outcity-perKM" && sp.ShippingUnitId== order.CoopID)
                        .Select(sp => new
                        {
                            ID = sp.PriceId,
                            Price = sp.Price,
                            StartDate = sp.StartDate,
                            PerKM = sp.PerKm
                        }).OrderByDescending(sp => sp.StartDate).ToList();

                    foreach (var item in perKMs)
                    {
                        if (item.StartDate/100<=order.OrderDate)
                        {
                            totalPrice += (decimal) (item.Price*((int)KM/item.PerKM));
                            break;
                        }
                    }
                }

                //calculate by voucher
                var orderVoucher = DBContext.OrderVouchers
                    .Where(ov => ov.OrderId==orderID).FirstOrDefault();
                if (orderVoucher != null)
                {
                    var voucher = DBContext.Vouchers
                        .Where(v => v.VoucherId==orderVoucher.VoucherId)
                        .Select(v => v.Percentage).FirstOrDefault();
                    totalPrice = totalPrice - (totalPrice*(decimal)voucher/100);
                }

                return totalPrice;
            }
        }
    }
}
