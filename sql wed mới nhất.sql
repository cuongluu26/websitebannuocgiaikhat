USE [GasSparkDB]
GO

CREATE TABLE DanhMucs (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    TenDanhMuc NVARCHAR(100) NOT NULL,
    MoTa       NVARCHAR(255),
    HinhAnh    NVARCHAR(500),
    ThuTu      INT DEFAULT 0,
    TrangThai  BIT DEFAULT 1
);
GO

CREATE TABLE Users (
    Id       INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50),
    Password NVARCHAR(50),
    Email    NVARCHAR(100),
    Role     NVARCHAR(20) DEFAULT 'user',
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);
CREATE TABLE SanPhams (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    TenSanPham NVARCHAR(200),
    Gia        INT,
    HinhAnh    NVARCHAR(200),
    MoTa       NVARCHAR(500),
    ThuongHieu NVARCHAR(100),
    DungTich   NVARCHAR(50),
    ThanhPhan  NVARCHAR(200),
    XuatXu     NVARCHAR(100),
    HanSuDung  NVARCHAR(50),
    Loai       NVARCHAR(100),
    IdDanhMuc  INT,
    CONSTRAINT FK_SanPhams_DanhMucs FOREIGN KEY (IdDanhMuc) REFERENCES DanhMucs(Id)
);
GO

CREATE TABLE YeuThiches (
    Id        INT IDENTITY(1,1) PRIMARY KEY,
    Username  NVARCHAR(100) NOT NULL,
    IdSanPham INT NOT NULL,
    FOREIGN KEY (IdSanPham) REFERENCES SanPhams(Id)
);
GO

CREATE TABLE DonHang (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Username    NVARCHAR(100)  NOT NULL,
    HoTen       NVARCHAR(200)  NOT NULL,
    SoDienThoai NVARCHAR(20)   NOT NULL,
    DiaChi      NVARCHAR(500)  NOT NULL,
    GhiChu      NVARCHAR(500)  NULL,
    TongTien    DECIMAL(18,0)  NOT NULL,
    NgayDat     DATETIME       NOT NULL DEFAULT GETDATE(),
    TrangThai   NVARCHAR(50)   NOT NULL DEFAULT N'Chờ xác nhận'
);
GO

CREATE TABLE ChiTietDonHang (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    IdDonHang  INT           NOT NULL,
    IdSanPham  INT           NOT NULL,
    TenSanPham NVARCHAR(200) NOT NULL,
    HinhAnh    NVARCHAR(200) NOT NULL,
    DonGia     DECIMAL(18,0) NOT NULL,
    SoLuong    INT           NOT NULL,
    FOREIGN KEY (IdDonHang) REFERENCES DonHang(Id),
    FOREIGN KEY (IdSanPham) REFERENCES SanPhams(Id)
);
GO
ALTER TABLE SanPhams
ALTER COLUMN Gia DECIMAL(18, 0) NOT NULL;

CREATE TABLE LienHe (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(100)  NOT NULL,
    Email       NVARCHAR(100)  NOT NULL,
    SoDienThoai NVARCHAR(20)   NULL,
    TieuDe      NVARCHAR(200)  NULL,
    NoiDung     NVARCHAR(1000) NOT NULL,
    NgayGui     DATETIME       DEFAULT GETDATE(),
    DaDoc       BIT            DEFAULT 0
);
GO

INSERT INTO dbo.Users (Username, Password, Email, Role)
SELECT v.Username, v.Password, v.Email, v.Role
FROM (VALUES
    (N'cuong',       N'cuong123$',     N'mannhimysun@gmail.com',                   N'admin'),
    (N'calim',       N'abcd0000@',     N'calim@gmail.com',                   N'user')
) AS v(Username, Password, Email, Role)
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Users AS u
    WHERE u.Username = v.Username
);

GO

INSERT INTO DanhMucs (TenDanhMuc, MoTa, ThuTu, TrangThai) VALUES
(N'Cà Phê',                              N'Các loại cà phê',         1, 1),
(N'Trà & Trà Sữa',                       N'Các loại trà và trà sữa', 2, 1),
(N'Nước Hạt É',                          N'Nước hạt é thiên nhiên',  3, 1),
(N'Nước Ép Trái Cây',                    N'Nước ép trái cây tươi',   4, 1),
(N'Nước Ép Trái Cây Có Thạch Cojo Cojo', N'Nước ép có thạch',        5, 1),
(N'Quà Tặng',                            N'Các combo quà tặng',      6, 1);
GO

INSERT INTO SanPhams (TenSanPham, Gia, HinhAnh, MoTa, ThuongHieu, DungTich, ThanhPhan, XuatXu, HanSuDung, Loai, IdDanhMuc) VALUES
(N'Cà Phê Đen Đá',        25000,  N'caphe1.jpg', N'Cà phê đen đậm đà, thơm ngon',        N'Gas Spark', N'250ml',    N'Cà phê nguyên chất',   N'Việt Nam', N'12 tháng', N'Cà Phê',    1),
(N'Cà Phê Sữa Đá',        30000,  N'caphe2.jpg', N'Cà phê sữa béo ngậy',                 N'Gas Spark', N'250ml',    N'Cà phê, sữa đặc',      N'Việt Nam', N'12 tháng', N'Cà Phê',    1),
(N'Cà Phê Muối',           32000,  N'caphe3.jpg', N'Cà phê muối đặc trưng Huế',           N'Gas Spark', N'250ml',    N'Cà phê, muối, kem',    N'Việt Nam', N'12 tháng', N'Cà Phê',    1),
(N'Cà Phê Dừa',            35000,  N'caphe4.jpg', N'Cà phê dừa thơm mát',                 N'Gas Spark', N'250ml',    N'Cà phê, nước cốt dừa', N'Việt Nam', N'12 tháng', N'Cà Phê',    1),
(N'Trà Sữa Trân Châu',     35000,  N'trasua1.jpg',N'Trà sữa trân châu đen thơm ngon',     N'Gas Spark', N'500ml',    N'Trà, sữa, trân châu',  N'Việt Nam', N'6 tháng',  N'Trà Sữa',   2),
(N'Trà Sữa Matcha',        38000,  N'trasua2.jpg',N'Trà sữa matcha Nhật Bản',              N'Gas Spark', N'500ml',    N'Matcha, sữa tươi',     N'Việt Nam', N'6 tháng',  N'Trà Sữa',   2),
(N'Trà Đào Cam Sả',        32000,  N'trasua3.jpg',N'Trà đào thơm mát thanh nhiệt',         N'Gas Spark', N'500ml',    N'Trà, đào, cam, sả',    N'Việt Nam', N'6 tháng',  N'Trà',       2),
(N'Trà Oolong Sữa',        36000,  N'trasua4.jpg',N'Trà oolong sữa béo ngậy',              N'Gas Spark', N'500ml',    N'Trà oolong, sữa',      N'Việt Nam', N'6 tháng',  N'Trà Sữa',   2),
(N'Trà Vải Thạch',         33000,  N'trasua5.jpg',N'Trà vải thạch mát lạnh',               N'Gas Spark', N'500ml',    N'Trà, vải, thạch',      N'Việt Nam', N'6 tháng',  N'Trà',       2),
(N'Nước Hạt É Nguyên Chất',20000,  N'hate1.jpg',  N'Nước hạt é tự nhiên thanh mát',       N'Gas Spark', N'330ml',    N'Hạt é, đường, nước',   N'Việt Nam', N'12 tháng', N'Nước Hạt É', 3),
(N'Nước Hạt É Chanh',      22000,  N'hate2.jpg',  N'Hạt é chanh chua ngọt',               N'Gas Spark', N'330ml',    N'Hạt é, chanh, đường',  N'Việt Nam', N'12 tháng', N'Nước Hạt É', 3),
(N'Nước Hạt É Dứa',        22000,  N'hate3.jpg',  N'Hạt é dứa thơm mát',                  N'Gas Spark', N'330ml',    N'Hạt é, dứa, đường',    N'Việt Nam', N'12 tháng', N'Nước Hạt É', 3),
(N'Nước Hạt É Dưa Hấu',    23000,  N'hate4.jpg',  N'Hạt é dưa hấu mùa hè',               N'Gas Spark', N'330ml',    N'Hạt é, dưa hấu',       N'Việt Nam', N'12 tháng', N'Nước Hạt É', 3),
(N'Nước Ép Cam Tươi',      28000,  N'ep1.jpg',    N'Cam tươi nguyên chất 100%',           N'Gas Spark', N'330ml',    N'Cam tươi',             N'Việt Nam', N'6 tháng',  N'Nước Ép',   4),
(N'Nước Ép Dưa Hấu',       25000,  N'ep2.jpg',    N'Dưa hấu mát lạnh ngày hè',            N'Gas Spark', N'330ml',    N'Dưa hấu tươi',         N'Việt Nam', N'6 tháng',  N'Nước Ép',   4),
(N'Nước Ép Táo',            30000,  N'ep3.jpg',    N'Táo xanh thanh mát',                  N'Gas Spark', N'330ml',    N'Táo tươi',             N'Việt Nam', N'6 tháng',  N'Nước Ép',   4),
(N'Nước Ép Xoài',           28000,  N'ep4.jpg',    N'Xoài chín thơm ngọt',                 N'Gas Spark', N'330ml',    N'Xoài tươi',            N'Việt Nam', N'6 tháng',  N'Nước Ép',   4),
(N'Nước Ép Dứa',            26000,  N'ep5.jpg',    N'Dứa tươi chua ngọt',                  N'Gas Spark', N'330ml',    N'Dứa tươi',             N'Việt Nam', N'6 tháng',  N'Nước Ép',   4),
(N'Nước Ép Cà Rốt',         27000,  N'ep6.jpg',    N'Cà rốt bổ dưỡng tốt cho mắt',        N'Gas Spark', N'330ml',    N'Cà rốt tươi',          N'Việt Nam', N'6 tháng',  N'Nước Ép',   4),
(N'Nước Ép Cam Thạch',     32000,  N'cojo1.jpg',  N'Nước ép cam có thạch dừa',            N'Cojo Cojo', N'330ml',    N'Cam, thạch dừa',       N'Việt Nam', N'12 tháng', N'Cojo Cojo', 5),
(N'Nước Ép Vải Thạch',     33000,  N'cojo2.jpg',  N'Vải thiều có thạch mát lạnh',         N'Cojo Cojo', N'330ml',    N'Vải, thạch',           N'Việt Nam', N'12 tháng', N'Cojo Cojo', 5),
(N'Nước Ép Dâu Thạch',     34000,  N'cojo3.jpg',  N'Dâu tây có thạch thơm ngon',          N'Cojo Cojo', N'330ml',    N'Dâu tây, thạch',       N'Việt Nam', N'12 tháng', N'Cojo Cojo', 5),
(N'Nước Ép Xoài Thạch',    33000,  N'cojo4.jpg',  N'Xoài chín có thạch béo ngọt',         N'Cojo Cojo', N'330ml',    N'Xoài, thạch dừa',      N'Việt Nam', N'12 tháng', N'Cojo Cojo', 5),
(N'Combo Cà Phê 4 Chai',   90000,  N'qua1.jpg',   N'Combo 4 chai cà phê các vị',          N'Gas Spark', N'4x250ml',  N'Cà phê',               N'Việt Nam', N'12 tháng', N'Quà Tặng',  6),
(N'Hộp Quà Trà Sữa',      120000,  N'qua2.jpg',   N'Hộp quà 4 hương vị trà sữa',         N'Gas Spark', N'4x500ml',  N'Trà, sữa',             N'Việt Nam', N'6 tháng',  N'Quà Tặng',  6),
(N'Combo Nước Ép Mix',     100000,  N'qua3.jpg',   N'Combo 5 loại nước ép trái cây',       N'Gas Spark', N'5x330ml',  N'Trái cây tươi',        N'Việt Nam', N'6 tháng',  N'Quà Tặng',  6),
(N'Hộp Quà Cao Cấp',      200000,  N'qua4.jpg',   N'Hộp quà đặc biệt đủ các dòng sản phẩm', N'Gas Spark', N'Nhiều loại', N'Đa dạng',         N'Việt Nam', N'12 tháng', N'Quà Tặng',  6);
GO

ALTER TABLE dbo.DonHang
ADD PhuongThucThanhToan NVARCHAR(20) NOT NULL
        CONSTRAINT DF_DonHang_PhuongThucThanhToan
        DEFAULT N'COD',
    TrangThaiThanhToan NVARCHAR(30) NOT NULL
        CONSTRAINT DF_DonHang_TrangThaiThanhToan
        DEFAULT N'Chưa thanh toán',
    NoiDungChuyenKhoan NVARCHAR(30) NULL;
GO
CREATE TABLE GioHang (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    Username   NVARCHAR(100) NOT NULL,
    SanPhamId  INT NOT NULL,
    TenSanPham NVARCHAR(255) NULL,
    HinhAnh    NVARCHAR(255) NULL,
    DonGia     DECIMAL(18,0) NOT NULL,
    SoLuong    INT NOT NULL
);
CREATE INDEX IX_GioHang_Username ON GioHang(Username);

CREATE TABLE ChatLichSu (
    Id       INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    VaiTro   NVARCHAR(10)  NOT NULL,   -- 'user' hoặc 'model'
    NoiDung  NVARCHAR(MAX) NOT NULL,
    ThoiGian DATETIME NOT NULL DEFAULT GETDATE()
);
CREATE INDEX IX_ChatLichSu_Username ON ChatLichSu(Username, Id);

IF OBJECT_ID('dbo.Voucher') IS NULL
CREATE TABLE dbo.Voucher (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Ma            NVARCHAR(50)  NOT NULL UNIQUE,
    TieuDe        NVARCHAR(200) NOT NULL,
    MoTa          NVARCHAR(500) NULL,
    LoaiGiam      NVARCHAR(20)  NOT NULL,          -- 'PhanTram' hoac 'SoTien'
    GiaTri        DECIMAL(18,0) NOT NULL,          -- % hoac so tien
    DonToiThieu   DECIMAL(18,0) NOT NULL DEFAULT 0,
    GiamToiDa     DECIMAL(18,0) NULL,              -- chi dung cho PhanTram
    NgayBatDau    DATETIME      NOT NULL,
    NgayKetThuc   DATETIME      NOT NULL,
    SoLuong       INT           NULL,              -- NULL = khong gioi han
    DaDung        INT           NOT NULL DEFAULT 0,
    MoiNguoiToiDa INT           NOT NULL DEFAULT 1,
    TrangThai     BIT           NOT NULL DEFAULT 1
);
GO

IF OBJECT_ID('dbo.VoucherUsage') IS NULL
CREATE TABLE dbo.VoucherUsage (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    VoucherId  INT           NOT NULL REFERENCES dbo.Voucher(Id),
    Username   NVARCHAR(100) NOT NULL,
    IdDonHang  INT           NOT NULL,
    NgaySuDung DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

IF COL_LENGTH('dbo.DonHang', 'MaVoucher') IS NULL
    ALTER TABLE dbo.DonHang ADD MaVoucher NVARCHAR(50) NULL;
GO
IF COL_LENGTH('dbo.DonHang', 'TienGiam') IS NULL
    ALTER TABLE dbo.DonHang ADD TienGiam DECIMAL(18,0) NOT NULL CONSTRAINT DF_DonHang_TienGiam DEFAULT 0;
GO

-- Du lieu mau
INSERT INTO dbo.Voucher (Ma, TieuDe, MoTa, LoaiGiam, GiaTri, DonToiThieu, GiamToiDa, NgayBatDau, NgayKetThuc, SoLuong, MoiNguoiToiDa)
VALUES
(N'GAS10',   N'Giảm 10% toàn bộ đơn hàng', N'Áp dụng cho mọi sản phẩm', N'PhanTram', 10, 100000, 50000, GETDATE(), DATEADD(MONTH, 3, GETDATE()), 500, 1),
(N'FREE20K', N'Giảm 20.000đ',              N'Cho đơn từ 150.000đ',       N'SoTien',   20000, 150000, NULL, GETDATE(), DATEADD(MONTH, 3, GETDATE()), NULL, 3);
GO
USE [GasSparkDB]
GO

INSERT INTO dbo.SanPhams (TenSanPham, Gia, HinhAnh, MoTa, ThuongHieu, DungTich, ThanhPhan, XuatXu, HanSuDung, Loai, IdDanhMuc)
SELECT v.TenSanPham, v.Gia, v.HinhAnh, v.MoTa, v.ThuongHieu, v.DungTich, v.ThanhPhan, v.XuatXu, v.HanSuDung, v.Loai, d.Id
FROM (VALUES
    -- ===== Cà Phê (thêm) =====
    (N'Bạc Xỉu',                    29000, N'caphe5.jpg',  N'Nhiều sữa, ít cà phê, ngọt dịu',              N'Gas Spark', N'250ml',   N'Cà phê, sữa đặc, sữa tươi',     N'Việt Nam', N'12 tháng', N'Cà Phê',      N'Cà Phê'),
    (N'Cold Brew Truyền Thống',     42000, N'caphe6.jpg',  N'Ủ lạnh 18 tiếng, vị êm ít đắng',              N'Gas Spark', N'330ml',   N'Cà phê Arabica, nước lọc',      N'Việt Nam', N'3 tháng',  N'Cà Phê',      N'Cà Phê'),
    (N'Cappuccino',                 45000, N'caphe7.jpg',  N'Espresso với lớp bọt sữa dày mịn',            N'Gas Spark', N'250ml',   N'Espresso, sữa tươi',            N'Việt Nam', N'6 tháng',  N'Cà Phê',      N'Cà Phê'),
    (N'Caramel Macchiato',          48000, N'caphe8.jpg',  N'Cà phê sữa sốt caramel thơm béo',             N'Gas Spark', N'300ml',   N'Cà phê, sữa, sốt caramel',      N'Việt Nam', N'6 tháng',  N'Cà Phê',      N'Cà Phê'),
    (N'Cà Phê Trứng',               45000, N'caphe9.jpg',  N'Lớp kem trứng béo mịn đặc sản Hà Nội',        N'Gas Spark', N'200ml',   N'Cà phê, lòng đỏ trứng, sữa đặc',N'Việt Nam', N'1 tháng',  N'Cà Phê',      N'Cà Phê'),

    -- ===== Trà & Trà Sữa (thêm) =====
    (N'Trà Chanh Giã Tay',          25000, N'trasua6.jpg', N'Trà chanh tươi chua nhẹ, giải khát',          N'Gas Spark', N'500ml',   N'Trà, chanh tươi, đường',        N'Việt Nam', N'6 tháng',  N'Trà',         N'Trà & Trà Sữa'),
    (N'Hồng Trà Macchiato',         39000, N'trasua7.jpg', N'Hồng trà kem macchiato béo mặn',              N'Gas Spark', N'500ml',   N'Hồng trà, kem cheese',          N'Việt Nam', N'6 tháng',  N'Trà Sữa',     N'Trà & Trà Sữa'),
    (N'Trà Sữa Thái Xanh',          34000, N'trasua8.jpg', N'Trà Thái xanh thơm lá dứa',                   N'Gas Spark', N'500ml',   N'Trà Thái, sữa',                 N'Việt Nam', N'6 tháng',  N'Trà Sữa',     N'Trà & Trà Sữa'),
    (N'Trà Sữa Khoai Môn',          37000, N'trasua9.jpg', N'Khoai môn nghiền bùi béo',                    N'Gas Spark', N'500ml',   N'Trà, sữa, khoai môn',           N'Việt Nam', N'6 tháng',  N'Trà Sữa',     N'Trà & Trà Sữa'),
    (N'Trà Sữa Socola Nóng',        40000, N'trasua10.jpg',N'Socola đậm vị kết hợp trà sữa',               N'Gas Spark', N'500ml',   N'Trà, sữa, socola',              N'Việt Nam', N'6 tháng',  N'Trà Sữa',     N'Trà & Trà Sữa'),
    (N'Trà Gạo Lứt',                28000, N'trasua11.jpg',N'Trà gạo lứt rang thơm, tốt cho sức khỏe',     N'Gas Spark', N'500ml',   N'Gạo lứt rang, nước lọc',        N'Việt Nam', N'6 tháng',  N'Trà',         N'Trà & Trà Sữa'),

    -- ===== Nước Hạt É (thêm) =====
    (N'Nước Hạt É Cam',             23000, N'hate5.jpg',   N'Hạt é cam vàng thơm mát',                     N'Gas Spark', N'330ml',   N'Hạt é, cam, đường',             N'Việt Nam', N'12 tháng', N'Nước Hạt É',  N'Nước Hạt É'),
    (N'Nước Hạt É Bạc Hà',          22000, N'hate6.jpg',   N'Hạt é bạc hà the mát',                        N'Gas Spark', N'330ml',   N'Hạt é, bạc hà, đường',          N'Việt Nam', N'12 tháng', N'Nước Hạt É',  N'Nước Hạt É'),
    (N'Nước Hạt É Mật Ong',         25000, N'hate7.jpg',   N'Hạt é mật ong thanh nhiệt',                   N'Gas Spark', N'330ml',   N'Hạt é, mật ong',                N'Việt Nam', N'12 tháng', N'Nước Hạt É',  N'Nước Hạt É'),
    (N'Nước Hạt É Chanh Dây',       24000, N'hate8.jpg',   N'Hạt é chanh dây chua ngọt',                   N'Gas Spark', N'330ml',   N'Hạt é, chanh dây, đường',       N'Việt Nam', N'12 tháng', N'Nước Hạt É',  N'Nước Hạt É'),

    -- ===== Nước Ép Trái Cây (thêm) =====
    (N'Nước Ép Ổi',                 27000, N'ep7.jpg',     N'Ổi hồng giàu vitamin C',                      N'Gas Spark', N'330ml',   N'Ổi tươi',                       N'Việt Nam', N'6 tháng',  N'Nước Ép',     N'Nước Ép Trái Cây'),
    (N'Nước Ép Lựu',                35000, N'ep8.jpg',     N'Lựu đỏ chống oxy hóa',                        N'Gas Spark', N'330ml',   N'Lựu tươi',                      N'Việt Nam', N'6 tháng',  N'Nước Ép',     N'Nước Ép Trái Cây'),
    (N'Nước Ép Cần Tây',            29000, N'ep9.jpg',     N'Cần tây thanh lọc cơ thể',                    N'Gas Spark', N'330ml',   N'Cần tây tươi',                  N'Việt Nam', N'6 tháng',  N'Nước Ép',     N'Nước Ép Trái Cây'),
    (N'Nước Ép Thanh Long Đỏ',      28000, N'ep10.jpg',    N'Thanh long ruột đỏ ngọt mát',                 N'Gas Spark', N'330ml',   N'Thanh long đỏ',                 N'Việt Nam', N'6 tháng',  N'Nước Ép',     N'Nước Ép Trái Cây'),
    (N'Nước Ép Detox Táo Cà Rốt',   32000, N'ep11.jpg',    N'Táo, cà rốt, cam kết hợp tăng đề kháng',      N'Gas Spark', N'330ml',   N'Táo, cà rốt, cam',              N'Việt Nam', N'6 tháng',  N'Nước Ép',     N'Nước Ép Trái Cây'),

    -- ===== Nước Ép Có Thạch Cojo Cojo (thêm) =====
    (N'Nước Ép Kiwi Thạch',         35000, N'cojo5.jpg',   N'Kiwi xanh chua nhẹ kèm thạch',                N'Cojo Cojo', N'330ml',   N'Kiwi, thạch',                   N'Việt Nam', N'12 tháng', N'Cojo Cojo',   N'Nước Ép Trái Cây Có Thạch Cojo Cojo'),
    (N'Nước Ép Đào Thạch',          33000, N'cojo6.jpg',   N'Đào vàng ngọt thanh kèm thạch',               N'Cojo Cojo', N'330ml',   N'Đào, thạch',                    N'Việt Nam', N'12 tháng', N'Cojo Cojo',   N'Nước Ép Trái Cây Có Thạch Cojo Cojo'),
    (N'Nước Ép Chanh Dây Thạch',    34000, N'cojo7.jpg',   N'Chanh dây chua thơm kèm thạch dừa',           N'Cojo Cojo', N'330ml',   N'Chanh dây, thạch dừa',          N'Việt Nam', N'12 tháng', N'Cojo Cojo',   N'Nước Ép Trái Cây Có Thạch Cojo Cojo'),
    (N'Nước Ép Việt Quất Thạch',    36000, N'cojo8.jpg',   N'Việt quất tím kèm thạch nha đam',             N'Cojo Cojo', N'330ml',   N'Việt quất, thạch nha đam',      N'Việt Nam', N'12 tháng', N'Cojo Cojo',   N'Nước Ép Trái Cây Có Thạch Cojo Cojo'),

    -- ===== Quà Tặng (thêm) =====
    (N'Combo Hạt É 6 Chai',        110000, N'qua5.jpg',    N'Combo 6 chai nước hạt é các vị',              N'Gas Spark', N'6x330ml', N'Hạt é, đường',                  N'Việt Nam', N'12 tháng', N'Quà Tặng',    N'Quà Tặng'),
    (N'Hộp Quà Cojo Cojo',         130000, N'qua6.jpg',    N'Hộp 4 chai nước ép có thạch Cojo Cojo',       N'Cojo Cojo', N'4x330ml', N'Trái cây, thạch',               N'Việt Nam', N'12 tháng', N'Quà Tặng',    N'Quà Tặng'),
    (N'Combo Văn Phòng 10 Ly',     280000, N'qua7.jpg',    N'10 ly đồ uống tùy chọn cho cả team',          N'Gas Spark', N'10x500ml',N'Đa dạng',                       N'Việt Nam', N'6 tháng',  N'Quà Tặng',    N'Quà Tặng')
) AS v(TenSanPham, Gia, HinhAnh, MoTa, ThuongHieu, DungTich, ThanhPhan, XuatXu, HanSuDung, Loai, TenDanhMuc)
JOIN dbo.DanhMucs d ON d.TenDanhMuc = v.TenDanhMuc
WHERE NOT EXISTS (SELECT 1 FROM dbo.SanPhams s WHERE s.TenSanPham = v.TenSanPham);
GO

INSERT INTO dbo.Voucher (Ma, TieuDe, MoTa, LoaiGiam, GiaTri, DonToiThieu, GiamToiDa, NgayBatDau, NgayKetThuc, SoLuong, DaDung, MoiNguoiToiDa, TrangThai)
SELECT v.Ma, v.TieuDe, v.MoTa, v.LoaiGiam, v.GiaTri, v.DonToiThieu, v.GiamToiDa, v.NgayBatDau, v.NgayKetThuc, v.SoLuong, v.DaDung, v.MoiNguoiToiDa, v.TrangThai
FROM (VALUES
    -- Khách hàng mới
    (N'WELCOME15', N'Chào bạn mới - Giảm 15%',        N'Dành cho khách hàng mới, đơn từ 80.000đ',            N'PhanTram', CAST(15 AS DECIMAL(18,0)),    CAST(80000 AS DECIMAL(18,0)),  CAST(30000 AS DECIMAL(18,0)),  GETDATE(),                   DATEADD(MONTH, 6, GETDATE()), 1000, 0, 1, 1),
    -- Theo danh mục / dịp
    (N'TRASUA10',  N'Ngày trà sữa - Giảm 10%',        N'Giảm 10% cho đơn từ 70.000đ',                        N'PhanTram', CAST(10 AS DECIMAL(18,0)),    CAST(70000 AS DECIMAL(18,0)),  CAST(20000 AS DECIMAL(18,0)),  GETDATE(),                   DATEADD(MONTH, 2, GETDATE()), 300,  0, 2, 1),
    (N'WEEKEND20', N'Cuối tuần vui vẻ - Giảm 20%',    N'Giảm 20% cho đơn từ 200.000đ, tối đa 80.000đ',       N'PhanTram', CAST(20 AS DECIMAL(18,0)),    CAST(200000 AS DECIMAL(18,0)), CAST(80000 AS DECIMAL(18,0)),  GETDATE(),                   DATEADD(DAY, 14, GETDATE()),  200,  0, 1, 1),
    (N'SUMMER25',  N'Hè sảng khoái - Giảm 25%',       N'Giảm 25% cho đơn từ 250.000đ, tối đa 100.000đ',      N'PhanTram', CAST(25 AS DECIMAL(18,0)),    CAST(250000 AS DECIMAL(18,0)), CAST(100000 AS DECIMAL(18,0)), GETDATE(),                   DATEADD(MONTH, 1, GETDATE()), 150,  0, 1, 1),
    -- Giảm số tiền cố định
    (N'COFFEE5K',  N'Giảm 5.000đ',                    N'Cho mọi đơn từ 50.000đ',                             N'SoTien',   CAST(5000 AS DECIMAL(18,0)),  CAST(50000 AS DECIMAL(18,0)),  NULL,                          GETDATE(),                   DATEADD(MONTH, 3, GETDATE()), NULL, 0, 5, 1),
    (N'SAVE30K',   N'Giảm 30.000đ',                   N'Cho đơn từ 200.000đ',                                N'SoTien',   CAST(30000 AS DECIMAL(18,0)), CAST(200000 AS DECIMAL(18,0)), NULL,                          GETDATE(),                   DATEADD(MONTH, 3, GETDATE()), 400,  0, 3, 1),
    (N'COMBO50K',  N'Mua combo giảm 50.000đ',         N'Cho đơn từ 300.000đ (phù hợp combo / hộp quà)',      N'SoTien',   CAST(50000 AS DECIMAL(18,0)), CAST(300000 AS DECIMAL(18,0)), NULL,                          GETDATE(),                   DATEADD(MONTH, 2, GETDATE()), 100,  0, 1, 1),
    (N'QUATANG40', N'Quà tặng ý nghĩa - Giảm 40.000đ',N'Cho đơn từ 400.000đ',                                N'SoTien',   CAST(40000 AS DECIMAL(18,0)), CAST(400000 AS DECIMAL(18,0)), NULL,                          GETDATE(),                   DATEADD(MONTH, 4, GETDATE()), 50,   0, 1, 1),
    -- Khách VIP / sự kiện lớn
    (N'VIP50',     N'Khách thân thiết - Giảm 50.000đ',N'Ưu đãi đặc biệt cho đơn từ 350.000đ',                N'SoTien',   CAST(50000 AS DECIMAL(18,0)), CAST(350000 AS DECIMAL(18,0)), NULL,                          GETDATE(),                   DATEADD(MONTH, 12, GETDATE()),NULL, 0, 10,1),
    (N'BLACKFRI30',N'Black Friday - Giảm 30%',        N'Giảm 30% cho đơn từ 150.000đ, tối đa 120.000đ',      N'PhanTram', CAST(30 AS DECIMAL(18,0)),    CAST(150000 AS DECIMAL(18,0)), CAST(120000 AS DECIMAL(18,0)), '2026-11-27 00:00:00',       '2026-11-30 23:59:59',        500,  0, 1, 1),
    -- Dữ liệu để TEST các trường hợp lỗi
    (N'HETHAN',    N'Voucher đã hết hạn (test)',      N'Dùng để kiểm tra trường hợp voucher hết hạn',        N'PhanTram', CAST(10 AS DECIMAL(18,0)),    CAST(0 AS DECIMAL(18,0)),      CAST(20000 AS DECIMAL(18,0)),  DATEADD(MONTH, -2, GETDATE()), DATEADD(DAY, -1, GETDATE()),  100,  0, 1, 1),
    (N'HETLUOT',   N'Voucher hết lượt (test)',        N'Dùng để kiểm tra trường hợp hết số lượng',           N'SoTien',   CAST(10000 AS DECIMAL(18,0)), CAST(0 AS DECIMAL(18,0)),      NULL,                          GETDATE(),                   DATEADD(MONTH, 1, GETDATE()), 10,   10,1, 1),
    (N'TAMDUNG',   N'Voucher tạm dừng (test)',        N'Dùng để kiểm tra trường hợp voucher bị tắt',         N'SoTien',   CAST(15000 AS DECIMAL(18,0)), CAST(0 AS DECIMAL(18,0)),      NULL,                          GETDATE(),                   DATEADD(MONTH, 1, GETDATE()), 100,  0, 1, 0)
) AS v(Ma, TieuDe, MoTa, LoaiGiam, GiaTri, DonToiThieu, GiamToiDa, NgayBatDau, NgayKetThuc, SoLuong, DaDung, MoiNguoiToiDa, TrangThai)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Voucher x WHERE x.Ma = v.Ma);
GO


-- 2. Thêm sản phẩm
DECLARE @IdDM INT = (SELECT TOP 1 Id FROM dbo.DanhMucs WHERE TenDanhMuc = N'Nước giải khát');

INSERT INTO dbo.SanPhams (TenSanPham, Gia, HinhAnh, MoTa, ThuongHieu, DungTich, ThanhPhan, XuatXu, HanSuDung, Loai, IdDanhMuc)
SELECT v.TenSanPham, v.Gia, v.HinhAnh, v.MoTa, v.ThuongHieu, v.DungTich, v.ThanhPhan, v.XuatXu, v.HanSuDung, v.Loai, @IdDM
FROM (VALUES
    -- ===== Cola =====
    (N'Coca-Cola Lon 330ml',            11000, N'ng1.jpg',  N'Nước ngọt có ga vị cola nguyên bản',               N'Coca-Cola', N'330ml',    N'Nước, đường, CO2, hương cola',          N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Coca-Cola Zero Lon 330ml',       11000, N'ng2.jpg',  N'Cola không đường',                                 N'Coca-Cola', N'330ml',    N'Nước, CO2, chất tạo ngọt',              N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Coca-Cola Chai 600ml',           12000, N'ng3.jpg',  N'Coca-Cola chai nhựa tiện mang đi',                 N'Coca-Cola', N'600ml',    N'Nước, đường, CO2, hương cola',          N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Coca-Cola Chai 1.5L',            24000, N'ng4.jpg',  N'Coca-Cola chai lớn cho gia đình',                  N'Coca-Cola', N'1.5L',     N'Nước, đường, CO2, hương cola',          N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Pepsi Lon 330ml',                10000, N'ng5.jpg',  N'Nước ngọt có ga vị cola',                          N'Pepsi',     N'330ml',    N'Nước, đường, CO2, hương cola',          N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Pepsi Không Calo Lon 330ml',     10000, N'ng6.jpg',  N'Pepsi không đường, không calo',                    N'Pepsi',     N'330ml',    N'Nước, CO2, chất tạo ngọt',              N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Pepsi Chai 1.5L',                23000, N'ng7.jpg',  N'Pepsi chai lớn',                                   N'Pepsi',     N'1.5L',     N'Nước, đường, CO2, hương cola',          N'Việt Nam', N'12 tháng', N'Nước Ngọt'),

    -- ===== Chanh / Lemon-lime =====
    (N'7Up Lon 330ml',                  10000, N'ng8.jpg',  N'Nước ngọt có ga vị chanh',                         N'7Up',       N'330ml',    N'Nước, đường, CO2, hương chanh',         N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'7Up Chai 1.5L',                  22000, N'ng9.jpg',  N'7Up chai lớn',                                     N'7Up',       N'1.5L',     N'Nước, đường, CO2, hương chanh',         N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Sprite Lon 330ml',               11000, N'ng10.jpg', N'Nước ngọt có ga vị chanh sảng khoái',              N'Sprite',    N'330ml',    N'Nước, đường, CO2, hương chanh',         N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Sprite Chai 600ml',              11000, N'ng11.jpg', N'Sprite chai nhựa tiện mang đi',                    N'Sprite',    N'600ml',    N'Nước, đường, CO2, hương chanh',         N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Sprite Chai 1.5L',               23000, N'ng12.jpg', N'Sprite chai lớn',                                  N'Sprite',    N'1.5L',     N'Nước, đường, CO2, hương chanh',         N'Việt Nam', N'12 tháng', N'Nước Ngọt'),

    -- ===== Cam / Trái cây có ga =====
    (N'Fanta Cam Lon 330ml',            11000, N'ng13.jpg', N'Nước ngọt có ga vị cam',                           N'Fanta',     N'330ml',    N'Nước, đường, CO2, hương cam',           N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Fanta Nho Lon 330ml',            11000, N'ng14.jpg', N'Nước ngọt có ga vị nho',                           N'Fanta',     N'330ml',    N'Nước, đường, CO2, hương nho',           N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Fanta Dâu Lon 330ml',            11000, N'ng15.jpg', N'Nước ngọt có ga vị dâu',                           N'Fanta',     N'330ml',    N'Nước, đường, CO2, hương dâu',           N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Mirinda Cam Lon 330ml',          10000, N'ng16.jpg', N'Nước ngọt có ga hương cam',                        N'Mirinda',   N'330ml',    N'Nước, đường, CO2, hương cam',           N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Mirinda Kem Soda Lon 330ml',     10000, N'ng17.jpg', N'Nước ngọt có ga hương kem soda',                   N'Mirinda',   N'330ml',    N'Nước, đường, CO2, hương kem soda',      N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Mirinda Xá Xị Lon 330ml',        10000, N'ng18.jpg', N'Nước ngọt có ga hương xá xị',                      N'Mirinda',   N'330ml',    N'Nước, đường, CO2, hương xá xị',         N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Schweppes Soda Lon 330ml',       13000, N'ng19.jpg', N'Soda tinh khiết dùng pha chế hoặc uống trực tiếp', N'Schweppes', N'330ml',    N'Nước, CO2',                             N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Schweppes Tonic Lon 330ml',      14000, N'ng20.jpg', N'Nước tonic vị đắng nhẹ đặc trưng',                 N'Schweppes', N'330ml',    N'Nước, đường, CO2, quinine',             N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Schweppes Ginger Ale Lon 330ml', 14000, N'ng21.jpg', N'Nước có ga vị gừng',                               N'Schweppes', N'330ml',    N'Nước, đường, CO2, chiết xuất gừng',     N'Việt Nam', N'12 tháng', N'Nước Ngọt'),

    -- ===== Nước tăng lực =====
    (N'Sting Dâu Chai 330ml',           10000, N'ng22.jpg', N'Nước tăng lực hương dâu',                          N'Sting',     N'330ml',    N'Nước, đường, taurine, hương dâu',       N'Việt Nam', N'12 tháng', N'Nước Tăng Lực'),
    (N'Sting Vàng Chai 330ml',          10000, N'ng23.jpg', N'Nước tăng lực hương trái cây nhiệt đới',           N'Sting',     N'330ml',    N'Nước, đường, taurine, hương trái cây',  N'Việt Nam', N'12 tháng', N'Nước Tăng Lực'),
    (N'Red Bull Lon 250ml',             14000, N'ng24.jpg', N'Nước tăng lực bổ sung năng lượng',                 N'Red Bull',  N'250ml',    N'Nước, đường, taurine, caffeine',        N'Thái Lan', N'24 tháng', N'Nước Tăng Lực'),
    (N'Number 1 Chai 330ml',            10000, N'ng25.jpg', N'Nước tăng lực vị truyền thống',                    N'Number 1',  N'330ml',    N'Nước, đường, taurine, vitamin B',       N'Việt Nam', N'12 tháng', N'Nước Tăng Lực'),
    (N'Warrior Dâu Chai 330ml',          9000, N'ng26.jpg', N'Nước tăng lực hương dâu',                          N'Warrior',   N'330ml',    N'Nước, đường, taurine, hương dâu',       N'Việt Nam', N'12 tháng', N'Nước Tăng Lực'),
    (N'Revive Chanh Muối 500ml',        11000, N'ng27.jpg', N'Nước bù khoáng vị chanh muối',                     N'Revive',    N'500ml',    N'Nước, đường, điện giải, chanh',         N'Việt Nam', N'12 tháng', N'Nước Bù Khoáng'),
    (N'Pocari Sweat 500ml',             16000, N'ng28.jpg', N'Nước bù ion thể thao',                             N'Pocari',    N'500ml',    N'Nước, đường, điện giải',                N'Việt Nam', N'12 tháng', N'Nước Bù Khoáng'),

    -- ===== Nước ngọt Gas Spark (nhãn riêng) =====
    (N'Gas Spark Cola Lon 330ml',        9000, N'ng29.jpg', N'Cola nhãn riêng Gas Spark',                        N'Gas Spark', N'330ml',    N'Nước, đường, CO2, hương cola',          N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Gas Spark Chanh Lon 330ml',       9000, N'ng30.jpg', N'Nước ngọt chanh nhãn riêng Gas Spark',             N'Gas Spark', N'330ml',    N'Nước, đường, CO2, hương chanh',         N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Gas Spark Cam Lon 330ml',         9000, N'ng31.jpg', N'Nước ngọt cam nhãn riêng Gas Spark',               N'Gas Spark', N'330ml',    N'Nước, đường, CO2, hương cam',           N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Gas Spark Soda Kem Lon 330ml',    9000, N'ng32.jpg', N'Soda kem nhãn riêng Gas Spark',                    N'Gas Spark', N'330ml',    N'Nước, đường, CO2, hương kem',           N'Việt Nam', N'12 tháng', N'Nước Ngọt'),

    -- ===== Trà / cà phê đóng chai =====
    (N'Trà Xanh Không Độ Chai 500ml',   10000, N'ng33.jpg', N'Trà xanh đóng chai vị ngọt nhẹ',                   N'Không Độ',  N'500ml',    N'Nước, trà xanh, đường',                 N'Việt Nam', N'9 tháng',  N'Trà Đóng Chai'),
    (N'C2 Trà Đào Chai 455ml',          10000, N'ng34.jpg', N'Trà đào đóng chai',                                N'C2',        N'455ml',    N'Nước, đường, trà, hương đào',           N'Việt Nam', N'9 tháng',  N'Trà Đóng Chai'),
    (N'Lipton Chanh Chai 455ml',        10000, N'ng35.jpg', N'Trà chanh đóng chai',                              N'Lipton',    N'455ml',    N'Nước, đường, trà, hương chanh',         N'Việt Nam', N'9 tháng',  N'Trà Đóng Chai'),
    (N'Dr Thanh Chai 450ml',            11000, N'ng36.jpg', N'Nước thảo mộc giải nhiệt',                         N'Dr Thanh',  N'450ml',    N'Nước, đường, thảo mộc',                 N'Việt Nam', N'12 tháng', N'Thảo Mộc'),
    (N'Cà Phê Sữa Lon 240ml',           12000, N'ng37.jpg', N'Cà phê sữa đóng lon uống liền',                    N'Wake-Up',   N'240ml',    N'Nước, cà phê, sữa, đường',              N'Việt Nam', N'12 tháng', N'Cà Phê Đóng Chai'),

    -- ===== Nước dừa / nước ép lon =====
    (N'Nước Dừa Cocoxim 330ml',         14000, N'ng38.jpg', N'Nước dừa đóng hộp có cùi dừa',                     N'Cocoxim',   N'330ml',    N'Nước dừa, cùi dừa',                     N'Việt Nam', N'12 tháng', N'Nước Dừa'),
    (N'Nước Ép Cam Lon 330ml',          12000, N'ng39.jpg', N'Nước ép cam đóng lon có tép',                      N'Gas Spark', N'330ml',    N'Nước, cam ép, đường',                   N'Việt Nam', N'12 tháng', N'Nước Ép Đóng Lon'),
    (N'Nước Ép Đào Lon 330ml',          12000, N'ng40.jpg', N'Nước ép đào đóng lon',                             N'Gas Spark', N'330ml',    N'Nước, đào, đường',                      N'Việt Nam', N'12 tháng', N'Nước Ép Đóng Lon'),

    -- ===== Lốc / Thùng =====
    (N'Lốc 6 Coca-Cola Lon',            60000, N'ng41.jpg', N'Lốc 6 lon Coca-Cola 330ml',                        N'Coca-Cola', N'6x330ml',  N'Nước, đường, CO2, hương cola',          N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Lốc 6 Pepsi Lon',                55000, N'ng42.jpg', N'Lốc 6 lon Pepsi 330ml',                            N'Pepsi',     N'6x330ml',  N'Nước, đường, CO2, hương cola',          N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Lốc 6 7Up Lon',                  55000, N'ng43.jpg', N'Lốc 6 lon 7Up 330ml',                              N'7Up',       N'6x330ml',  N'Nước, đường, CO2, hương chanh',         N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Lốc 6 Sting Dâu',                55000, N'ng44.jpg', N'Lốc 6 chai Sting dâu 330ml',                       N'Sting',     N'6x330ml',  N'Nước, đường, taurine, hương dâu',       N'Việt Nam', N'12 tháng', N'Nước Tăng Lực'),
    (N'Thùng 24 Coca-Cola Lon',        235000, N'ng45.jpg', N'Thùng 24 lon Coca-Cola 330ml',                     N'Coca-Cola', N'24x330ml', N'Nước, đường, CO2, hương cola',          N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Thùng 24 Pepsi Lon',            220000, N'ng46.jpg', N'Thùng 24 lon Pepsi 330ml',                         N'Pepsi',     N'24x330ml', N'Nước, đường, CO2, hương cola',          N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Thùng 24 7Up Lon',              220000, N'ng47.jpg', N'Thùng 24 lon 7Up 330ml',                           N'7Up',       N'24x330ml', N'Nước, đường, CO2, hương chanh',         N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Thùng 24 Fanta Cam Lon',        230000, N'ng48.jpg', N'Thùng 24 lon Fanta cam 330ml',                     N'Fanta',     N'24x330ml', N'Nước, đường, CO2, hương cam',           N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Combo Nước Ngọt Mix 12 Lon',    125000, N'ng49.jpg', N'12 lon mix Coca, Pepsi, 7Up, Fanta',               N'Gas Spark', N'12x330ml', N'Nước ngọt có ga các loại',              N'Việt Nam', N'12 tháng', N'Nước Ngọt'),
    (N'Combo Tiệc 48 Lon Mix',         430000, N'ng50.jpg', N'48 lon nước ngọt mix cho tiệc, sinh nhật',         N'Gas Spark', N'48x330ml', N'Nước ngọt có ga các loại',              N'Việt Nam', N'12 tháng', N'Nước Ngọt')
) AS v(TenSanPham, Gia, HinhAnh, MoTa, ThuongHieu, DungTich, ThanhPhan, XuatXu, HanSuDung, Loai)
WHERE @IdDM IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.SanPhams s WHERE s.TenSanPham = v.TenSanPham);

PRINT N'Đã thêm: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' sản phẩm vào danh mục Nước giải khát (Id = ' + CAST(@IdDM AS NVARCHAR(10)) + N')';
GO

UPDATE Voucher SET HinhAnh = LOWER(Ma) + '.png'
WHERE Ma IN ('WEEKEND20','BLACKFRI30','SUMMER25','COMBO50K','TRASUA10','GAS10',
             'FREE20K','COFFEE5K','SAVE30K','QUATANG40','WELCOME15','VIP50');