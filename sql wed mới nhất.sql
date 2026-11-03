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
    (N'giabaone123', N'11072006Vvv@',  N'tranbao78900@gmail.com',                  N'user'),
    (N'baokun123',   N'11072006Vvv@',  N'2424802010006@student.tdmu.edu.vn',       N'user'),
    (N'cuong',       N'cuong123$',     N'mannhimysun@gmail.com',                   N'admin'),
    (N'calim',       N'abcd0000@',     N'calim@gmail.com',                   N'user'),
    (N'nguyen123',   N'11072006Vvv@@', N'2424802010009@student.tdmu.edu.vn',       N'user')
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