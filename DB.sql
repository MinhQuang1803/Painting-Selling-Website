CREATE DATABASE WebSellingPainting
go
USE WebSellingPainting
go
CREATE TABLE Discount(
	DiscountID INT PRIMARY KEY IDENTITY
	, [Percentage] INT
	, StartDate INT
	, EndDate INT
);
CREATE TABLE [Type](
	TypeID INT PRIMARY KEY IDENTITY
	, TypeName VARCHAR(50)
);
CREATE TABLE [Status](
	StatusID INT PRIMARY KEY IDENTITY
	, StatusName NVARCHAR(50)
	, TypeID INT REFERENCES [Type](TypeID)
);
CREATE TABLE [Role](
	RoleID INT PRIMARY KEY IDENTITY
	, RoleName VARCHAR(50)
);
CREATE TABLE [Address](
	AddressID INT PRIMARY KEY IDENTITY
	, City NVARCHAR(50)
	, District NVARCHAR(50)
	, Street NVARCHAR(50)
	, Detail NVARCHAR(MAX)
	, Status INT REFERENCES [Status](StatusID)
	, ObjectID INT 
	, TypeID INT REFERENCES [Type](TypeID)
);
CREATE TABLE [Image](
	ImageID INT PRIMARY KEY IDENTITY
	, ImageUrl VARCHAR(MAX)
	, ObjectID INT
	, TypeID INT REFERENCES [Type](TypeID)
);
CREATE TABLE Category(
    CategoryID INT PRIMARY KEY IDENTITY
	, CategoryName NVARCHAR(50)
	, [Description] NVARCHAR(MAX)
	, TypeID INT REFERENCES [Type](TypeID)
	, StatusID INT REFERENCES Status(StatusID)
);
CREATE TABLE Account(
	AccountID INT PRIMARY KEY IDENTITY
	, Email NVARCHAR(50)
	, [Password] NVARCHAR(MAX)
	, RoleID INT REFERENCES [Role](RoleID)
	, StatusID INT REFERENCES Status(StatusID)
);
CREATE TABLE [User](
	UserID INT PRIMARY KEY IDENTITY
	, UserName NVARCHAR(MAX)
	, Gender BIT 
	, PhoneNumber VARCHAR(15)
	, ShopName NVARCHAR(MAX)
	, ArtistBackground NVARCHAR(MAX)
	, AccountID INT REFERENCES Account(AccountID)
);
CREATE TABLE Voucher(
	VoucherID INT PRIMARY KEY IDENTITY
	, VoucherName NVARCHAR(50)
	, VoucherCode VARCHAR(50)
	, StartDate INT
	, EndDate INT
	, [Percentage] DECIMAL
	, MinOrderValue DECIMAL
	, UserID INT REFERENCES [User](UserID)
	, StatusID INT REFERENCES [Status](StatusID)
);
CREATE TABLE Comment(
	CommentID INT PRIMARY KEY IDENTITY
	, Content NVARCHAR(MAX)
	, CommentDate Datetime   
	, ObjectID INT
	, TypeID INT REFERENCES [Type](TypeID)
	, UserID INT REFERENCES [User](UserID)
	, CommentRepID INT REFERENCES Comment(CommentID)
);
CREATE TABLE Painting(
	PaintingID INT PRIMARY KEY IDENTITY
	, [Name] NVARCHAR(100)
	, Price DECIMAL
	, [Description] NVARCHAR(MAX)
	, Height DECIMAL
	, Width DECIMAL
	, Quantity INT
	, ViewCount INT
	, PublishDate INT
	, UserID INT REFERENCES [User](UserID)
	, StatusID INT REFERENCES [Status](StatusID)
	, DiscountID INT REFERENCES Discount(DiscountID)
	, IsFragile BIT 
);
CREATE TABLE ShippingUnit(
	ShippingUnitID INT PRIMARY KEY IDENTITY
	, [Name] NVARCHAR(MAX)
	, PhoneNumber VARCHAR(15)
	, Email VARCHAR(30)
	, Website VARCHAR(MAX)
	, StatusID INT REFERENCES [Status](StatusID)
);
CREATE TABLE Post(
	PostID INT PRIMARY KEY IDENTITY
	, Title NVARCHAR(50)
	, Content NVARCHAR(MAX)
	, [Date] INT
	, ViewCount INT
	, UserID INT REFERENCES [User](UserID)
	, StatusID INT REFERENCES [Status](StatusID)
);
CREATE TABLE PostCategory(
	PostID INT REFERENCES Post(PostID)
	, CategoryID INT REFERENCES Category(CategoryID)
	PRIMARY KEY(PostID, CategoryID)
);
CREATE TABLE ShippingPrice(
	PriceID INT PRIMARY KEY IDENTITY
	, ShippingUnitID INT REFERENCES ShippingUnit(ShippingUnitID)
	, Price DECIMAL
	, TypeID INT REFERENCES Type(TypeID)
	, PerKM INT 
	, StartDate INT 
);
CREATE TABLE [Order](
	OrderID INT PRIMARY KEY IDENTITY
	, OrderNote NVARCHAR(MAX)
	, OrderDate INT
	, ShipDate INT
	, ReceiverName NVARCHAR(50)
	, PhoneNumber VARCHAR(15)
	, PaymentMethod NVARCHAR(50)
	, UserID INT REFERENCES [User](UserID)
	, ShippingUnitID INT REFERENCES ShippingUnit(ShippingUnitID)
	, StatusID INT REFERENCES [Status](StatusID)
	, FromAddressID INT REFERENCES [Address](AddressID)
	, ToAddressID INT REFERENCES [Address](AddressID)
	, OrderCode NVARCHAR(30)
);
CREATE TABLE OrderDetail(
	OrderDetailID INT PRIMARY KEY IDENTITY
	, Quantity INT
	, StatusID INT REFERENCES [Status](StatusID)
	, ReadyDate INT
	, Discount INT
	, PaintingID INT REFERENCES Painting(PaintingID)
	, OrderID INT REFERENCES [Order](OrderID)
	, Price DECIMAL
);
CREATE TABLE Report(
	ReportID INT PRIMARY KEY IDENTITY
	, ReportDate INT
	, ReportNote NVARCHAR(MAX)
	, TypeID INT REFERENCES [Type](TypeID)
	, ObjectID INT
	, StatusID INT REFERENCES [Status](StatusID)
	, UserID INT REFERENCES [User](UserID)
	, SupervisorID INT REFERENCES [User](UserID)
);
CREATE TABLE ReportCategory(
	ReportID INT REFERENCES Report(ReportID)
	, CategoryID INT REFERENCES Category(CategoryID)
	PRIMARY KEY(ReportID, CategoryID)
);

CREATE TABLE PaintingCategory(
	PaintingID INT REFERENCES Painting(PaintingID)
	, CategoryID INT REFERENCES Category(CategoryID)
	PRIMARY KEY(PaintingID, CategoryID)
);

CREATE TABLE Cart(
	UserID INT REFERENCES [User](UserID),
	PaintingID INT REFERENCES Painting(PaintingID)
	PRIMARY KEY(UserID, PaintingID)
);

CREATE TABLE TransactionHistory(
	TransactionID INT PRIMARY KEY IDENTITY,
	PaymentID NVARCHAR(MAX),
	[Time] INT,
	Content NVARCHAR(MAX),
	Amount DECIMAL,
	[StatusID] INT,
	[TypeID] INT REFERENCES [Type](TypeID)
);


CREATE TABLE PayAccount(
	PayAccountId INT PRIMARY KEY IDENTITY,
	AccountId INT REFERENCES Account(AccountID),
	ActiveAccountDate INT,
	ExpiredAccountDate INT,
	ActiveCode NVARCHAR(MAX),
	ActiveFee DECIMAL,
	StatusId INT REFERENCES Status(StatusID)
	)

CREATE TABLE UserTransaction(
UserTransactionId  INT PRIMARY KEY IDENTITY,
TransactionId INT REFERENCES [TransactionHistory] (TransactionID),
PayAccountId INT REFERENCES [PayAccount] (PayAccountId),
UserId INT REFERENCES [User](UserID)
);

CREATE TABLE OrderVoucher(
	UserID INT REFERENCES [User](UserID),
	OrderID INT REFERENCES [Order](OrderID),
	VoucherID INT REFERENCES Voucher(VoucherID)
	PRIMARY KEY(UserID, OrderID, VoucherID)
);
---------------------------------------------------------------------------------------
GO
--function to get status id base on type name and status name
CREATE FUNCTION GetStatusID
(
    @TypeName NVARCHAR(50),
    @StatusName NVARCHAR(50)
)
RETURNS INT
AS
BEGIN
    DECLARE @StatusID INT;

    SELECT @StatusID = s.StatusID
    FROM [Status] s
    JOIN [Type] t ON s.TypeID = t.TypeID
    WHERE t.TypeName = @TypeName AND s.StatusName = @StatusName;

    RETURN @StatusID;
END;
GO
---------------------------------------------------------------------------------------
INSERT INTO [Role] VALUES 
('Customer'),
('Admin'),
('Artist'),
('Manager'),
('Supervisor');

INSERT INTO [Type] VALUES
('Account'),
('Post'),
('Painting'),
('Slider'),
('Report'),
('Cooperator'),
('User'),
('Address'),
('Category'),
('Order'),
('OrderDetail'),
('Incity-normal'),--id12
('Incity-heavy'),
('Incity-normal-fragile'),
('Incity-heavy-fragile'),
('Outcity-normal'),
('Outcity-heavy'),--id17
('Outcity-normal-fragile'),
('Outcity-heavy-fragile'),
('Outcity-perKM'),
('Promotion'),
('ExtendArtistAccount'),
('OrderPay'),
('UpgradeArtistPay');

INSERT INTO [Status] VALUES
('Enable', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Account')),
('Disable', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Account')),
('Default', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Address')),
('Active', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Address')),
('Inactive', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Address')),
('Active', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('Inactive', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('Waiting', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('Hidden', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('Active', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Category')),
('Inactive', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Category')),
('Active', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post')),
('Inactive', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post')),
('Hidden', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post')),
('Waiting', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post')),
('Active', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Cooperator')),--id 16
('Inactive', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Cooperator')),
('Processing', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Order')),
('Delivering', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Order')),
('Successfully', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Order')),
('Canceled', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Order')),
('Returned', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Order')),
('Processing', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'OrderDetail')),
('Delivering', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'OrderDetail')),
('Delivered', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'OrderDetail')),
('Returned', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'OrderDetail')),
('Active', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Promotion')),
('Inactive', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Promotion')),
('Transaction Completed', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'UpgradeArtistPay')),
('Not Paid Artist Account', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Account')),
('Expired Artist Account', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Account')),
('Paid And Active', (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Account'))
;

INSERT INTO Account VALUES 
('customer@gmail.com', 'bGFwdHJpbmh2aWVu', 1, 1),
('admin@gmail.com', 'bGFwdHJpbmh2aWVu', 2, 1),
('artist@gmail.com', 'bGFwdHJpbmh2aWVu', 3, 1),
('manager@gmail.com', 'bGFwdHJpbmh2aWVu', 4, 1),
('supervisor@gmail.com', 'bGFwdHJpbmh2aWVu', 5, 1),
('artist2@gmail.com', 'bGFwdHJpbmh2aWVu', 3, 1);

INSERT INTO [User]
VALUES 
('Xuan Thanh', 1, '123456789', 'JD', '', 1),
('Minh Quang', 0, '987654321', 'JS', '', 2),
('Thuan Thanh', 1, '555555555', 'MJ', N'Tốt nghiệp loại giỏi Đại học Mỹ Thuật Công nghiệp.
 Giải nhì cuộc thi và triển lãm mỹ thuật Việt Nam 2023. Có nhiều sản phẩm đã bán
 được hàng trăm bản.', 3),
('Minh Quan', 0, '777777777', 'EB', '', 4),
('Huy Son', 1, '999999999', 'CL', '', 5),
('Lilith', 0, '0397527913', 'PinkGirl', N'Tốt nghiệp ngành Mỹ thuật số tại đại học FPT.
 tham gia nhiều cuộc thi vẽ tranh cho sinh viên Đại học.', 6);

INSERT INTO Discount VALUES
(10, 20230404, 20230414),
(20, 20230104, 20230204),
(5, 20231212, 20240412),
(12, 20240101, 20240401),
(10, 20240101, 20240131),
(0, 20240101, 20240131),
(5, 20240101, 20240131),
(10, 20240101, 20240131),
(10, 20240101, 20240131),
(5, 20240101, 20240131);

INSERT INTO Painting VALUES
(N'Angel', 100000, N'Picture of a perfect angel', 15, 15, 6, 20, 20240214, 6, 
dbo.GetStatusID('Painting', 'Active'), 1, 0),
(N'Demon', 90000, N'Picture of a demon', 13, 13, 6, 40, 20230414, 6, 
dbo.GetStatusID('Painting', 'Active'), 2, 1),
(N'Thiếu nữ bên hoa huệ', 200000, N'Tranh nàng thiếu nữ', 15, 15, 6, 25, 20230515, 6, 
dbo.GetStatusID('Painting', 'Active'), 3, 0),
(N'Làng quê tôi', 100000, N'Làng tôi thời chiến', 15, 10, 6, 56, 20240114, 6, 
dbo.GetStatusID('Painting', 'Active'), 4, 1),
(N'Con heo béo', 50000, N'Con heo nhà nuôi được :3', 20, 20, 6, 34, 20231214, 6, 
dbo.GetStatusID('Painting', 'Active'), 5, 1),
(N'Hà Nội phố', 300000, N'Cùng nhìn lại thành phố của chúng ta nhé', 15, 15, 5, 45, 20240815, 3, 
dbo.GetStatusID('Painting', 'Active'), 6, 0),
(N'Hercules', 500000, N'Our mighty hero, son of great Zeus', 15, 20, 5, 10, 20230414, 3, 
dbo.GetStatusID('Painting', 'Active'), 7, 1),
(N'Mama', 100000, N'The one true mum', 15, 15, 5, 33, 20231224, 3, 
dbo.GetStatusID('Painting', 'Active'), 8, 0),
(N'Papa', 100000, N'The one true pap', 20, 20, 5, 22, 20231124, 3, 
dbo.GetStatusID('Painting', 'Active'), 9, 1),
(N'Lotus Girl', 100000, N'What an art', 20, 20, 5, 17, 20240101, 3, 
dbo.GetStatusID('Painting', 'Active'), 10, 1)

INSERT INTO [Image](ImageUrl, ObjectID, TypeID) VALUES
('/Image/Picture/angel1.jfif', 1, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('/Image/Picture/demon.jpg', 2, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('/Image/Picture/thieunuhoahue1.jpg', 3, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('/Image/Picture/thieunuhoahue2.jpg', 3, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('/Image/Picture/thieunuhoahue3.jpg', 3, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('/Image/Picture/langquetoi.jpg', 4, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('/Image/Picture/conheobeo.jpg', 5, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('/Image/Picture/hanoipho.jpg', 6, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('/Image/Picture/hercules.jpg', 7, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('/Image/Picture/mama.jpg', 8, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('/Image/Picture/papa.jpg', 9, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('/Image/Picture/lotuslady.jpg', 10, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting')),
('/Image/User/cat1.jpg', 1, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'User')),
('/Image/User/cat2.jpg', 2, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'User')),
('/Image/User/dino.jpg', 3, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'User')),
('/Image/User/dragon.jpg', 4, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'User')),
('/Image/User/ironman.jpg', 5, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'User')),
('/Image/Post/bull.jpg', 1, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post')),
('/Image/Post/mona.jpg', 1, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post')),
('/Image/Post/city.jpg', 1, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post')),
('/Image/Post/home1.jpg', 4, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post')),
('/Image/Post/peacock.jpg', 4, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post')),
('/Image/User/ninjavan.jpg', 1, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Cooperator')),
('/Image/User/ghn.jpg', 2, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Cooperator')),
('/Image/Post/post1.jpg', 2, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post')),
('/Image/Post/post2.jpg', 3, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post')),
('/Image/User/artist2.jpg', 6, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'User')),
('/Image/Logo/voucher.jpg', 1, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Promotion')),
('/Image/Logo/voucher3.jpg', 2, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Promotion')),
('/Image/Logo/voucher3.jpg', 3, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Promotion')),
('/Image/Logo/voucher.jpg', 4, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Promotion')),
('/Image/Logo/voucher.jpg', 5, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Promotion')),
('/Image/Logo/voucher.jpg', 6, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Promotion')),
('/Image/Logo/voucher.jpg', 7, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Promotion')),
('/Image/Logo/voucher2.jpg', 8, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Promotion')),
('/Image/Logo/voucher2.jpg', 9, (SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Promotion'))



INSERT INTO Category VALUES 
('Landscape', 'Scenery of natural landscapes such as mountains, forests, and rivers.', 
(SELECT TypeID FROM Type WHERE TypeName = 'Painting'), dbo.GetStatusID('Category', 'Active')),
('Portrait', 'Depictions of individuals, usually focusing on their face and expression.', 
(SELECT TypeID FROM Type WHERE TypeName = 'Painting'), dbo.GetStatusID('Category', 'Active')),
('Still Life', 'Arrangements of inanimate objects such as fruit, flowers, and everyday items.', 
(SELECT TypeID FROM Type WHERE TypeName = 'Painting'), dbo.GetStatusID('Category', 'Active')),
('Abstract', 'Non-representational artworks that prioritize form, color, and texture.', 
(SELECT TypeID FROM Type WHERE TypeName = 'Painting'), dbo.GetStatusID('Category', 'Active')),
('Animal', 'Artworks featuring various animals, including pets, wildlife, and mythical creatures.', 
(SELECT TypeID FROM Type WHERE TypeName = 'Painting'), dbo.GetStatusID('Category', 'Active')),
('Introduce painting', 'Description for Introduce painting category',
(SELECT TypeID FROM Type WHERE TypeName = 'Post'), dbo.GetStatusID('Category', 'Active')),
('Find painting', 'Description for Find painting category', 
(SELECT TypeID FROM Type WHERE TypeName = 'Post'), dbo.GetStatusID('Category', 'Active')),
('Art tips', 'Description for Art tips category',
(SELECT TypeID FROM Type WHERE TypeName = 'Post'), dbo.GetStatusID('Category', 'Active')),
('Painting techniques', 'Description for Painting techniques category', 
(SELECT TypeID FROM Type WHERE TypeName = 'Post'), dbo.GetStatusID('Category', 'Active')),
('Art events', 'Description for Art events category', 
(SELECT TypeID FROM Type WHERE TypeName = 'Post'), dbo.GetStatusID('Category', 'Active'));
;

INSERT INTO Category VALUES 
('Expressionism', 'Scenery of natural landscapes such as mountains, forests, and rivers.', 
(SELECT TypeID FROM Type WHERE TypeName = 'Painting'), dbo.GetStatusID('Category', 'Active'))


INSERT INTO PaintingCategory VALUES
(1, 2), (1, 4), (2, 2), (2, 4), (3, 2), (3, 3), (4, 3), (4, 1), (5, 5),
(6, 1), (6, 3), (7, 2), (8, 2), (9, 2), (10, 2)

INSERT INTO [Address] VALUES
(N'Thành phố Hà Nội', N'Quận Thanh Xuân', N'Phường Nhân Chính', N'Nhà 20 ngõ 15', 
dbo.GetStatusID('Address', 'Default'), 1, (SELECT TypeID FROM Type WHERE TypeName = 'User')),
(N'Thành phố Hà Nội', N'Quận Nam Từ Liêm', N'Phường Cầu Diễn', N'Nhà 37 ngõ 150', 
dbo.GetStatusID('Address', 'Active'), 1, (SELECT TypeID FROM Type WHERE TypeName = 'User')),
(N'Thành phố Hà Nội', N'Quận Hai Bà Trưng', N'Phường Đồng Tâm', N'Trường tiểu học Bạch Mai', 
dbo.GetStatusID('Address', 'Active'), 1, (SELECT TypeID FROM Type WHERE TypeName = 'User')),
(N'Thành phố Hà Nội', N'Quận Hai Bà Trưng', N'Phường Đồng Tâm', N'Trường tiểu học Bạch Mai', 
dbo.GetStatusID('Address', 'Default'), 3, (SELECT TypeID FROM Type WHERE TypeName = 'User')),
(N'Thành phố Hà Nội', N'Quận Hai Bà Trưng', N'Phường Bạch Mai', N'Chợ Đồng Tâm', 
dbo.GetStatusID('Address', 'Active'), 3, (SELECT TypeID FROM Type WHERE TypeName = 'User')),
(N'Tỉnh Hà Giang', N'Thành phố Hà Giang', N'Phường Quang Trung', N'nhà 102', 
dbo.GetStatusID('Address', 'Default'), 6, (SELECT TypeID FROM Type WHERE TypeName = 'User')),
(N'Tỉnh Hà Giang', N'Huyện Đồng Văn', N'Xã Lũng Cú', N'nhà 130', 
dbo.GetStatusID('Address', 'Active'), 6, (SELECT TypeID FROM Type WHERE TypeName = 'User'))


INSERT INTO [Post] VALUES
('History of art', 
'The history of art focuses on objects made by humans for any number of spiritual, narrative, philosophical, symbolic, conceptual, documentary, decorative, and even functional and other purposes, but with a primary emphasis on its aesthetic visual form. Visual art can be classified in diverse ways, such as separating fine arts from applied arts; inclusively focusing on human creativity; or focusing on different media such as architecture, sculpture, painting, film, photography, and graphic arts. In recent years, technological advances have led to video art, computer art, performance art, animation, television, and videogames.
The history of art is often told as a chronology of masterpieces created during each civilization. It can thus be framed as a story of high culture, epitomized by the Wonders of the World. On the other hand, vernacular art expressions can also be integrated into art historical narratives, referred to as folk arts or craft. The more closely that an art historian engages with these latter forms of low culture, the more likely it is that they will identify their work as examining visual culture or material culture, or as contributing to fields related to art history, such as anthropology or archaeology. In the latter cases, art objects may be referred to as archeological artifacts.', 
20230414, 0, 1, dbo.GetStatusID('Post', 'Active')),
('Finding a picture for coffe shop',
'I want to find some picture to decorate my coffe store, i want it to has bright color, main theme is coffe or the feeling of chill in a coffe shop.',
20230515, 0, 1, dbo.GetStatusID('Post', 'Active')),
('Finding decor pic for new house',
'I want to find a picture for our new house. I want 5 pics that has dark color, IT theme for me and 5 pics has brighter color and a bit for children for my wife. Feel free to leave contact', 
20230809, 0, 1, dbo.GetStatusID('Post', 'Active')),
('Presenting new picture for your home',
'Today i will present to all customer these pictures of mine, they are color full, beautiful and have all size for you to choose. Please have a look and leave a comment to let me know which one is your favorite.',
20230606, 0, 3, dbo.GetStatusID('Post', 'Active'));

INSERT INTO PostCategory(PostID, CategoryID) VALUES
(1, 6), (2, 7), (3, 7), (4, 6), (4, 10)

INSERT INTO Comment VALUES
('Wow, history are interesting', '2023-01-01 01:01:01', 1, 
(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting'), 1, null),
('I know right? Cant wait to know more', '2023-01-01 02:02:02', 1, 
(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting'), 2, 1),
('They start painting since ice age, lol :)', '2023-01-01 03:03:03', 1, 
(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting'), 3, 1),
('hm, i wonder if anyone sell a fake Monalisa, i would buy it :>', '2023-01-01 04:04:04', 1, 
(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting'), 4, 1),
(N'Người xưa vẽ đẹp với ý nghĩa thật, ko như máy tranh trừu tượng bây h @_@', '2023-02-02 01:01:01', 1, 
(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting'), 5, null),
('Learn how to appreciate art', '2023-02-02 02:02:02', 1, 
(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting'), 1, null),
('Haha, sounds like a boss in a meme video said', '2023-02-02 03:03:03', 1,
(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Painting'), 2, 6);

--INSERT INTO Comment VALUES
--('Wow, history are interesting', '2023-01-01 01:01:01', 1, 
--(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post'), 1, null),
--('I know right? Cant wait to know more', '2023-01-01 02:02:02', 1, 
--(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post'), 2, 11),
--('Look how beautiful Mona Lisa is', '2023-01-01 03:03:03', 1, 
--(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post'), 3, 11),
--('hm, i wonder if anyone sell a fake Monalisa, i would buy it :>', '2023-01-01 04:04:04', 1, 
--(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post'), 4, null),
--(N'Người xưa vẽ đẹp với ý nghĩa thật, ko như máy tranh trừu tượng bây h @_@', '2023-02-02 01:01:01', 1, 
--(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post'), 5, null),
--('Im not sure if that is legal, hmmm', '2023-02-02 02:02:02', 1, 
--(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post'), 1, 14),
--('I like the way you show the history to us', '2023-02-02 03:03:03', 1,
--(SELECT t.TypeID FROM [Type] t WHERE t.TypeName = 'Post'), 2, null);

INSERT INTO ShippingUnit VALUES
('ninajavan', '84 1900 886 877', 'ninjavan@gmail.com', 'https://www.ninjavan.co/vi-vn/', 16),
('giaohangnhanh', '1900 636677', 'cskh@ghn.vn', 'https://ghn.vn/', 16)

INSERT INTO [Order] VALUES
(N'Giao cho tôi thứ 6 nhé', '20231212', '20231215', 'Xuan Thanh', '0397527913',
 'COD', 1, 1, 19, 4, 1, 'yplplmbnvnbv'),
(N'Giao kín cho tôi', '20240412', '20240415', 'Xuan Thanh', '0397527913',
'COD', 1, 1, 18, 4, 1, 'cczxczcxcx'),
(N'', '20231223', '20231225', 'Hong Luong', '0397527913',
'Online', 1, 1, 20, 4, 1, 'zxczxczvzvxv'),
(N'Nếu tôi ko có nhà thì hãy để vào khe cửa', '20240212', '20240215', 
'Xuan Thanh', '0397527913', 'COD', 1, 1, 19, 4, 1, 'zxczxczxvv'),
(N'Order for month ', '20240301', '20240305', 'Receiver 1', '0397527913', 'COD', 1, 1, 20, 4, 1, 'qưewqewretry'),
(N'Order for month ', '20240301', '20240314', 'Receiver 2', '0397527913', 'COD', 1, 1, 21, 4, 1, 'fghjklpiiooo'),
(N'Order for month ', '20240323', '20240325', 'Receiver 3', '0397527913', 'OnlineBanking', 1, 1, 20, 4, 1, 'asdlp;hjf'),
(N'Order for month ', '20240315', '20240318', 'Receiver 4', '0397527913', 'COD', 1, 1, 20, 4, 1, 'zxvcbvnbvmcv'),
(N'Order Note 9', '20231201', '20231204', 'Receiver 1', '1234567890', 'COD', 1, 1, 17, 4, 1, 'asghjklsdasd'),
(N'Order Note 10', '20231208', '', 'Receiver 2', '1234567891', 'OnlineBanking', 1, 2, 18, 4, 1, 'qưeqweqeweqq'),
(N'Order Note 11', '20231215', '', 'Receiver 3', '1234567892', 'COD', 1, 1, 19, 4, 1, 'xcsdferigjivm'),
(N'Order Note 12', '20231222', '20231225', 'Receiver 4', '1234567893', 'OnlineBanking', 1, 2, 20, 4, 1, 'âsdasds'),
(N'Order Note 13', '20231229', '20240101', 'Receiver 5', '1234567894', 'COD', 1, 1, 21, 4, 1, 'qưeqweqewyh');

-- Orders for January 2024
INSERT INTO [Order] VALUES
(N'Order Note 14', '20240105', '20240108', 'Receiver 6', '1234567895', 'OnlineBanking', 1, 2, 20, 4, 1, 'dfsdfsdwefrw'),
(N'Order Note 15', '20240112', '20240115', 'Receiver 7', '1234567896', 'COD', 1, 1, 20, 4, 1, 'peokgpmbirewf'),
(N'Order Note 16', '20240119', '', 'Receiver 8', '1234567897', 'OnlineBanking', 1, 2, 18, 4, 1, 'zxvcvxbvbczvn'),
(N'Order Note 17', '20240126', '', 'Receiver 9', '1234567898', 'COD', 1, 1, 19, 4, 1, 'zvxcbcnfgm'),
(N'Order Note 18', '20240131', '20240203', 'Receiver 10', '1234567899', 'OnlineBanking', 1, 2, 20, 4, 1, 'sdgfhfdfdgg');

-- Orders for February 2024
INSERT INTO [Order] VALUES
(N'Order Note 19', '20240207', '20240210', 'Receiver 11', '1234567800', 'COD', 1, 1, 21, 4, 1, 'ewfwemwpgmerp0'),
(N'Order Note 20', '20240214', '20240217', 'Receiver 12', '1234567801', 'OnlineBanking', 1, 2, 22, 4, 1, 'dsdvewrvd'),
(N'Order Note 21', '20240221', '20240224', 'Receiver 13', '1234567802', 'COD', 1, 1, 20, 4, 1, 'fmiwvmiedvwe'),
(N'Order Note 22', '20240228', '', 'Receiver 14', '1234567803', 'OnlineBanking', 1, 2, 18, 4, 1, 'deqiwrasojv');

INSERT INTO OrderDetail VALUES
(3, 24, 20231213, 10, 1, 1, 100000),
(2, 24, 20231213, 0, 3, 1, 120000),
(1, 24, 20231213, 15, 4, 1, 90000),
(3, 24, 20231213, 0, 7, 1, 200000),
(1, 24, 20231213, 15, 5, 2, 150000),
(1, 24, 20231213, 0, 3, 2, 110000),
(1, 24, 20231213, 5, 1, 3, 110000),
(1, 24, 20231213, 0, 6, 3, 200000),
(1, 24, 20231213, 10, 2, 4, 95000),
(1, 24, 20231213, 0, 5, 4, 130000),
(1, 24, '20240303', 10, 1, 5, 100000),
(1, 24, '20240303', 5, 3, 5, 100000),
(1, 24, '20240303', 10, 4, 5, 92000),
(1, 24, '20240305', 0, 6, 6, 250000),
(1, 24, '20240324', 0, 6, 7, 50000),
(1, 24, '20240324', 0, 8, 7, 300000),
(1, 24, '20240317', 5, 8, 8, 120000),
(1, 24, '20240317', 5, 2, 8, 50000),
(1, 24, '20231202', 5, 2, 9, 80000),
(1, 24, '20231202', 3, 1, 9, 200000),
(1, 24, '20231210', 6, 2, 10, 145000),
(1, 24, '20231210', 20, 7, 10, 450000),
(1, 24, '20231217', 0, 2, 11, 68000),
(1, 24, '20231217', 0, 9, 11, 90000),
(1, 24, '20231223', 2, 10, 12, 160000),
(1, 24, '20231223', 9, 4, 12, 135000),
(1, 24, '20231230', 5, 4, 13, 220000),
(1, 24, '20231230', 5, 9, 13, 230000),
(1, 24, '20240106', 5, 9, 14, 155000),
(1, 24, '20240106', 15, 3, 14, 100000),
(1, 24, '20240106', 5, 5, 14, 90000),
(1, 24, '20240113', 0, 9, 15, 100000),
(1, 24, '20240120', 5, 6, 16, 95000),
(1, 24, '20240120', 10, 7, 16, 110000),
(1, 24, '20240120', 0, 1, 16, 125000),
(1, 24, '20240120', 5, 4, 16, 125000),
(1, 24, '20240127', 0, 1, 17, 130000),
(1, 24, '20240127', 0, 7, 17, 120000),
(1, 24, '20240127', 5, 8, 17, 200000),
(1, 24, '20240127', 0, 3, 17, 130000),
(1, 24, '20240201', 0, 1, 18, 100000),
(1, 24, '20240201', 0, 9, 18, 150000),
(1, 24, '20240208', 10, 10, 19, 400000),
(1, 24, '20240215', 10, 6, 20, 200000),
(1, 24, '20240215', 10, 2, 20, 340000),
(1, 24, '20240215', 10, 7, 20, 340000),
(1, 24, '20240222', 0, 7, 21, 300000),
(1, 24, '20240222', 0, 9, 21, 300000),
(1, 24, '20240222', 0, 3, 21, 250000),
(1, 24, '20240222', 0, 1, 21, 150000),
(2, 24, '20240301', 10, 1, 22, 170000)


INSERT INTO Cart VALUES
(1, 3), (1, 4), (1, 7), (1, 5), (1, 8)

INSERT INTO ShippingPrice VALUES
(1, 30000, 12, 0, 202201),
(1, 35000, 13, 0, 202201),
(1, 40000, 14, 0, 202201),
(1, 45000, 15, 0, 202201),
(1, 33000, 16, 0, 202201),
(1, 38000, 17, 0, 202201),
(1, 43000, 18, 0, 202201),
(1, 48000, 19, 0, 202201),
(1, 3000, 20, 10, 202201),
(2, 30000, 12, 0, 202201),
(2, 35000, 13, 0, 202201),
(2, 40000, 14, 0, 202201),
(2, 45000, 15, 0, 202201),
(2, 33000, 16, 0, 202201),
(2, 38000, 17, 0, 202201),
(2, 43000, 18, 0, 202201),
(2, 48000, 19, 0, 202201),
(2, 3000, 20, 10, 202301),
(2, 32000, 12, 0, 202301),
(2, 37000, 13, 0, 202301),
(2, 42000, 14, 0, 202301),
(2, 47000, 15, 0, 202301),
(2, 35000, 16, 0, 202301),
(2, 40000, 17, 0, 202301),
(2, 45000, 18, 0, 202301),
(2, 50000, 19, 0, 202301),
(2, 5000, 20, 100, 202301)

INSERT INTO Voucher VALUES
('Discount 10%','DV10','20230328','20230331','10','100000','3', (SELECT S.StatusID FROM [Status] AS S INNER JOIN [Type] AS T ON  S.TypeID = T.TypeID WHERE T.TypeName = 'Promotion' and S.StatusName='Active')),
('Discount 50%','DV50','20220328','20220331','20','500000','3', (SELECT S.StatusID FROM [Status] AS S INNER JOIN [Type] AS T ON  S.TypeID = T.TypeID WHERE T.TypeName = 'Promotion' and S.StatusName='Active')),
('Discount 60%','DV60','20230528','20230531','15','250000','3', (SELECT S.StatusID FROM [Status] AS S INNER JOIN [Type] AS T ON  S.TypeID = T.TypeID WHERE T.TypeName = 'Promotion' and S.StatusName='Active')),
('Discount 70%','DV70','20240318','20240321','25','1000000','3', (SELECT S.StatusID FROM [Status] AS S INNER JOIN [Type] AS T ON  S.TypeID = T.TypeID WHERE T.TypeName = 'Promotion' and S.StatusName='Active')),
('Discount 80%','DV80','20231218','20231225','20','900000','3', (SELECT S.StatusID FROM [Status] AS S INNER JOIN [Type] AS T ON  S.TypeID = T.TypeID WHERE T.TypeName = 'Promotion' and S.StatusName='Active')),
('Discount 90%','DV90','20230414','20230425','10','150000','6', (SELECT S.StatusID FROM [Status] AS S INNER JOIN [Type] AS T ON  S.TypeID = T.TypeID WHERE T.TypeName = 'Promotion' and S.StatusName='Active')),
('Discount 20%','DV20','20240128','20240131','20','90000','6', (SELECT S.StatusID FROM [Status] AS S INNER JOIN [Type] AS T ON  S.TypeID = T.TypeID WHERE T.TypeName = 'Promotion' and S.StatusName='Active')),
('Discount 30%','DV30','20240328','20240525','20','900000','6', (SELECT S.StatusID FROM [Status] AS S INNER JOIN [Type] AS T ON  S.TypeID = T.TypeID WHERE T.TypeName = 'Promotion' and S.StatusName='Active')),
('Discount 40%','DV40','20240401','20240525','5','100000','6', (SELECT S.StatusID FROM [Status] AS S INNER JOIN [Type] AS T ON  S.TypeID = T.TypeID WHERE T.TypeName = 'Promotion' and S.StatusName='Active'))

INSERT INTO PayAccount VALUES
(3, 20220101, 20250101, 'fadadasdasd', 300000, 32),
(6, 20220101, 20250101, 'dfdfdfdf', 300000, 32)