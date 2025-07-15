using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_ProductTable_Add_Field_IsPremium : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GlobalId",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPremium",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProductGlobalConfigStr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Code", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DescriptionStr", "EventIdsStr", "ExpireDate", "GlobalId", "ImagesStr", "IsDeleted", "IsPremium", "MarketPlaceType", "Name", "Price", "ProductGlobalConfigStr", "ProductType", "Quantity", "ShowPriority", "Status", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("0defe395-1de1-4371-b163-c99484ed66a4"), "CardVinaphone50000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3716", "[\"https://images.urbox.vn/_img_server/2021/05/07/640/1620372109_6094ea8da37a4.png\"]", false, true, "UrBox", "[UrBox Voucher] Thẻ nạp Vinaphone 50.000đ", 5000, "{\"brandName\":\"Vinaphone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721341_5e842ffdee2fc.png\",\"content\":null,\"note\":null}", "Voucher", 88, true, "Active", null, null, null },
                    { new Guid("1dd2462c-22a6-4319-95b0-a1bf9f719e0d"), "VoucherCircleK20000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "12555", "[\"https://images.urbox.vn/_img_server/2024/08/22/640/1724320850_66c70c52eb0a1.png\"]", false, true, "UrBox", "[UrBox Voucher] Circle K 20.000đ", 2000, "{\"brandName\":\"Circle K\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585713650_5e8411f295a7b.png\",\"content\":null,\"note\":null}", "Voucher", 750, true, "Active", null, null, null },
                    { new Guid("27885469-9843-49f6-92ad-c5ae0d84ae71"), "CardVietnamobile20000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3741", "[\"https://images.urbox.vn/_img_server/2021/05/10/640/1620633130_6098e62b11e0f.png\"]", false, true, "UrBox", "[UrBox Voucher] Thẻ nạp Vietnamobile 20.000đ", 2000, "{\"brandName\":\"Vietnamobile\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721231_5e842f8f7bde0.png\",\"content\":null,\"note\":null}", "Voucher", 187, true, "Active", null, null, null },
                    { new Guid("2b28eb9a-432d-436e-9968-42b47c89a12f"), "CardMobifone20000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3723", "[\"https://images.urbox.vn/_img_server/2021/05/07/640/1620369744_6094e15089073.png\"]", false, true, "UrBox", "[UrBox Voucher] Thẻ nạp Mobifone 20.000đ", 2000, "{\"brandName\":\"Mobifone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585718650_5e84257b025ac.png\",\"content\":null,\"note\":null}", "Voucher", 187, true, "Active", null, null, null },
                    { new Guid("3114a892-4cef-49ec-8cd6-94785c83386f"), "CardVinaphone10000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3713", "[\"https://images.urbox.vn/_img_server/2021/05/07/640/1620372055_6094ea5788d2f.png\"]", false, true, "UrBox", "[UrBox Voucher] Thẻ nạp Vinaphone 10.000đ", 1000, "{\"brandName\":\"Vinaphone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721341_5e842ffdee2fc.png\",\"content\":null,\"note\":null}", "Voucher", 300, true, "Active", null, null, null },
                    { new Guid("3c641e97-1a87-4f63-b94c-08693d3d66a7"), "CardViettel50000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3707", "[\"https://images.urbox.vn/_img_server/2021/05/07/640/1620371187_6094e6f42048b.png\"]", false, true, "UrBox", "[UrBox Voucher] Thẻ nạp Viettel 50.000đ", 5000, "{\"brandName\":\"Viettel\",\"brandImage\":\"https://images.urbox.vn/_img_server/2021/07/16/160/1626425459_60f14873d5d12.png\",\"content\":null,\"note\":null}", "Voucher", 88, true, "Active", null, null, null },
                    { new Guid("42cb4a24-6bfb-4eb3-b454-8a583024cb06"), "CardViettel20000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3705", "[\"https://images.urbox.vn/_img_server/2021/05/07/640/1620371151_6094e6cf53c22.png\"]", false, true, "UrBox", "[UrBox Voucher] Thẻ nạp Viettel 20.000đ", 2000, "{\"brandName\":\"Viettel\",\"brandImage\":\"https://images.urbox.vn/_img_server/2021/07/16/160/1626425459_60f14873d5d12.png\",\"content\":null,\"note\":null}", "Voucher", 188, true, "Active", null, null, null },
                    { new Guid("48f395b0-441a-4c24-aa28-de7a654c8a80"), "EVMuaHeTuHocMacbookAirM2", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[\"Học viên đổi số Xu FSEL (FSEL Coin) tương ứng trên Marketplace sẽ nhận được 01 mã đổi quà. Sau khi đổi thành công vui lòng lưu lại mã quà tặng để đối chiếu khi được yêu cầu.\",\"Thời gian nhận quà: Sau ngay nhận mã đổi thưởng, vui lòng liên hệ hotline 0906259442 để xác thực thông tin và nhận quà\",\"Hình thức nhận quà: FSEL chuyển quà tới học viên qua địa chỉ học sinh cung cấp\"],\"conditions\":[\"Quà không có giá trị quy đổi thành tiền mặt\",\"Mỗi mã đổi quà tương ứng với 01 sản phẩm\",\"Số lượng quà tự động giảm dần theo số lượng học sinh đã đổi quà\",\"Chỉ những học sinh mua khóa học trong khoảng thời gian từ ngày 01/06/2025 đến hết ngày 30/06/2025 mới đủ điều kiện nhận quà.\",\"Các tài khoản được tặng khóa học miễn phí, bao gồm tài khoản được tặng thêm tháng học hoặc sử dụng voucher giảm giá 100%, sẽ không đủ điều kiện để đổi quà.\"],\"contacts\":[\"Mọi thắc mắc liên hệ hotline: 0906 259 442\",\"Email: support_fsel@atlantic.edu.vn\"],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "[\"https://anphat.com.vn/media/product/50827_laptop_apple_macbook_air_13_inch_m2_8cpu8gpu16gb256gb___space_grey_mc7u4saa__1_.jpg\",\"https://anphat.com.vn/media/product/50827_laptop_apple_macbook_air_13_inch_m2_8cpu8gpu16gb256gb___space_grey_mc7u4saa__4_.jpg\",\"https://anphat.com.vn/media/product/50827_laptop_apple_macbook_air_13_inch_m2_8cpu8gpu16gb256gb___space_grey_mc7u4saa__3_.jpg\"]", false, true, "FSEL", "Laptop Apple MacBook Air 13 inch M2 8CPU/8GPU/16GB/256GB - Space Grey MC7U4SA/A - Chính Hãng Apple Việt Nam", 150000, null, "Physical", 1, true, "Active", null, null, null },
                    { new Guid("4f13c2f5-d2f7-4395-b2c7-c97e9ff9ef89"), "EVMuaHeTuHocAirPod", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[\"Học viên đổi số Xu FSEL (FSEL Coin) tương ứng trên Marketplace sẽ nhận được 01 mã đổi quà. Sau khi đổi thành công vui lòng lưu lại mã quà tặng để đối chiếu khi được yêu cầu.\",\"Thời gian nhận quà: Sau ngay nhận mã đổi thưởng, vui lòng liên hệ hotline 0906259442 để xác thực thông tin và nhận quà\",\"Hình thức nhận quà: FSEL chuyển quà tới học viên qua địa chỉ học sinh cung cấp\"],\"conditions\":[\"Quà không có giá trị quy đổi thành tiền mặt\",\"Mỗi mã đổi quà tương ứng với 01 sản phẩm\",\"Số lượng quà tự động giảm dần theo số lượng học sinh đã đổi quà\",\"Chỉ những học sinh mua khóa học trong khoảng thời gian từ ngày 01/06/2025 đến hết ngày 30/06/2025 mới đủ điều kiện nhận quà.\",\"Các tài khoản được tặng khóa học miễn phí, bao gồm tài khoản được tặng thêm tháng học hoặc sử dụng voucher giảm giá 100%, sẽ không đủ điều kiện để đổi quà.\"],\"contacts\":[\"Mọi thắc mắc liên hệ hotline: 0906 259 442\",\"Email: support_fsel@atlantic.edu.vn\"],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "[\"https://www.maccenter.vn/Accessories/Apple-AirPods4-A.jpg\",\"https://cdn.viettelstore.vn/Images/Product/ProductImage/479432236.jpeg\",\"https://cdn2.cellphones.com.vn/x/media/catalog/product/a/i/airpods-4-chong-on-7.png\"]", false, true, "FSEL", "Tai nghe Apple Airpods 4 (Chống ồn chủ động)", 25000, null, "Physical", 1, true, "Active", null, null, null },
                    { new Guid("50781c44-617a-49e2-a7a3-a239178495bc"), "CardMobifone50000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3725", "[\"https://images.urbox.vn/_img_server/2021/05/07/640/1620369780_6094e174274c6.png\"]", false, true, "UrBox", "[UrBox Voucher] Thẻ nạp Mobifone 50.000đ", 5000, "{\"brandName\":\"Mobifone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585718650_5e84257b025ac.png\",\"content\":null,\"note\":null}", "Voucher", 87, true, "Active", null, null, null },
                    { new Guid("6d756ac2-33d8-4c21-8f31-3f4c67f50a14"), "EVMuaHeTuHocIphone", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[\"Học viên đổi số Xu FSEL (FSEL Coin) tương ứng trên Marketplace sẽ nhận được 01 mã đổi quà. Sau khi đổi thành công vui lòng lưu lại mã quà tặng để đối chiếu khi được yêu cầu.\",\"Thời gian nhận quà: Sau ngay nhận mã đổi thưởng, vui lòng liên hệ hotline 0906259442 để xác thực thông tin và nhận quà\",\"Hình thức nhận quà: FSEL chuyển quà tới học viên qua địa chỉ học sinh cung cấp\"],\"conditions\":[\"Quà không có giá trị quy đổi thành tiền mặt\",\"Mỗi mã đổi quà tương ứng với 01 sản phẩm\",\"Số lượng quà tự động giảm dần theo số lượng học sinh đã đổi quà\",\"Chỉ những học sinh mua khóa học trong khoảng thời gian từ ngày 01/06/2025 đến hết ngày 30/06/2025 mới đủ điều kiện nhận quà.\",\"Các tài khoản được tặng khóa học miễn phí, bao gồm tài khoản được tặng thêm tháng học hoặc sử dụng voucher giảm giá 100%, sẽ không đủ điều kiện để đổi quà.\"],\"contacts\":[\"Mọi thắc mắc liên hệ hotline: 0906 259 442\",\"Email: support_fsel@atlantic.edu.vn\"],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "[\"https://cdn-media.sforum.vn/storage/app/media/wp-content/uploads/2023/10/iPhone-15-pro-max-titan-xanh-6.jpg\"]", false, true, "FSEL", "Iphone 15 Pro 128Gb Titan xanh", 100000, null, "Physical", 1, true, "Active", null, null, null },
                    { new Guid("9d449591-6cc6-4e2f-b75c-521a7c68edb9"), "CardMobifone10000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3722", "[\"https://images.urbox.vn/_img_server/2021/05/07/640/1620369223_6094df47d677d.png\"]", false, true, "UrBox", "[UrBox Voucher] Thẻ nạp Mobifone 10.000đ", 1000, "{\"brandName\":\"Mobifone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585718650_5e84257b025ac.png\",\"content\":null,\"note\":null}", "Voucher", 300, true, "Active", null, null, null },
                    { new Guid("ab2e9d9b-dfc1-47fa-8f7a-1e039478a20b"), "CardVietnamobile50000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3743", "[\"https://images.urbox.vn/_img_server/2021/05/10/640/1620633619_6098e813aac42.png\"]", false, true, "UrBox", "[UrBox Voucher] Thẻ nạp Vietnamobile 50.000đ", 5000, "{\"brandName\":\"Vietnamobile\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721231_5e842f8f7bde0.png\",\"content\":null,\"note\":null}", "Voucher", 87, true, "Active", null, null, null },
                    { new Guid("c21b61be-ce79-4c02-9fb8-54fc6d482a40"), "VoucherCircleK50000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "12558", "[\"https://images.urbox.vn/_img_server/2024/08/22/640/1724320916_66c70c9489e1f.png\"]", false, true, "UrBox", "[UrBox Voucher] Circle K 50.000đ", 5000, "{\"brandName\":\"Circle K\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585713650_5e8411f295a7b.png\",\"content\":null,\"note\":null}", "Voucher", 350, true, "Active", null, null, null },
                    { new Guid("da1ff94d-14dc-4b4a-bb81-2ff5420049a3"), "CardVinaphone20000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3714", "[\"https://images.urbox.vn/_img_server/2021/05/07/640/1620372074_6094ea6a75828.png\"]", false, true, "UrBox", "[UrBox Voucher] Thẻ nạp Vinaphone 20.000đ", 2000, "{\"brandName\":\"Vinaphone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721341_5e842ffdee2fc.png\",\"content\":null,\"note\":null}", "Voucher", 188, true, "Active", null, null, null },
                    { new Guid("fc29bd59-2220-4bf3-b003-5f0ca116a634"), "CardViettel10000", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", null, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "3704", "[\"https://images.urbox.vn/_img_server/2021/05/07/640/1620371136_6094e6c0d04b4.png\"]", false, true, "UrBox", "[UrBox Voucher] Thẻ nạp Viettel 10.000đ", 1000, "{\"brandName\":\"Viettel\",\"brandImage\":\"https://images.urbox.vn/_img_server/2021/07/16/160/1626425459_60f14873d5d12.png\",\"content\":null,\"note\":null}", "Voucher", 400, true, "Active", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "ProductTranslations",
                columns: new[] { "Id", "CreatedDate", "CreatedFullName", "CreatedUserId", "DeletedDate", "DeletedFullName", "DeletedUserId", "DescriptionStr", "IsDeleted", "Language", "Name", "ProductId", "UpdatedDate", "UpdatedFullName", "UpdatedUserId" },
                values: new object[,]
                {
                    { new Guid("001ec6cc-b059-403f-be40-da89a476732b"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] Mobifone Mobile Card 10,000 VND", new Guid("9d449591-6cc6-4e2f-b75c-521a7c68edb9"), null, null, null },
                    { new Guid("0dac99b5-4c11-44d7-98a5-2fd5fcff06e8"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] Vinaphone Mobile Card 10,000 VND", new Guid("3114a892-4cef-49ec-8cd6-94785c83386f"), null, null, null },
                    { new Guid("22fd9daa-d9ec-48c9-b009-de390cc7efd3"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Thẻ nạp Mobifone 10.000đ", new Guid("9d449591-6cc6-4e2f-b75c-521a7c68edb9"), null, null, null },
                    { new Guid("29e66472-b20e-4b1f-aa7d-c2bd2b2c0f74"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Thẻ nạp Vietnamobile 20.000đ", new Guid("27885469-9843-49f6-92ad-c5ae0d84ae71"), null, null, null },
                    { new Guid("2d19bc52-35ef-44a7-b6bc-6fa45b99c0cc"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[\"Học viên đổi số Xu FSEL (FSEL Coin) tương ứng trên Marketplace sẽ nhận được 01 mã đổi quà.\",\"Sau khi đổi thành công vui lòng lưu lại mã quà tặng để đối chiếu khi được yêu cầu.\",\"Thời gian nhận quà: Sau ngay nhận mã đổi thưởng, vui lòng liên hệ hotline 0906259442 để xác thực thông tin và nhận quà\",\"Hình thức nhận quà: FSEL chuyển quà tới học viên qua địa chỉ học sinh cung cấp\"],\"conditions\":[\"Quà không có giá trị quy đổi thành tiền mặt\",\"Mỗi mã đổi quà tương ứng với 01 sản phẩm\",\"Số lượng quà tự động giảm dần theo số lượng học sinh đã đổi quà\",\"Chỉ những học sinh mua khóa học trong khoảng thời gian từ ngày 01/06/2025 đến hết ngày 30/06/2025 mới đủ điều kiện nhận quà.\",\"Các tài khoản được tặng khóa học miễn phí, bao gồm tài khoản được tặng thêm tháng học hoặc sử dụng voucher giảm giá 100%, sẽ không đủ điều kiện để đổi quà.\"],\"contacts\":[\"Mọi thắc mắc liên hệ hotline: 0906 259 442\",\"Email: support_fsel@atlantic.edu.vn\"],\"others\":null}", false, "vi-VN", "Iphone 15 Pro 128Gb Titan xanh", new Guid("6d756ac2-33d8-4c21-8f31-3f4c67f50a14"), null, null, null },
                    { new Guid("2ed60c84-7245-4e62-a328-ca6055085a70"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[\"Learners who exchange FSEL Coins on the Marketplace will receive one (01) gift redemption code.\",\"Upon successful redemption, please save the code for verification purposes when requested.\",\"Gift collection timeline: Immediately after receiving the redemption code, please contact the hotline at 0906 259 442 to verify your information and claim your gift.\",\"Gift delivery method: FSEL will deliver the gift to the address provided by the learner.\"],\"conditions\":[\"The gift has no cash value\",\"Each gift redemption code corresponds to a product quantity of 1\",\"The number of gifts is limited according to the document \\\"Self-study month\\\" announced by the D0ET of NgheAn\",\"The number of gifts automatically decreases according to the number of students who have redeemed the gift\",\"Only students who purchase a course between June 1, 2025 and June 30, 2025 are eligible to receive gifts.\",\"Accounts that received a free course—such as those granted additional study time or using a 100% discount voucher—are not eligible for gift redemption.\"],\"contacts\":[\"For any questions, please contact hotline: 0906 259 442\",\"Email: support_fsel@atlantic.edu.vn\"],\"others\":null}", false, "en-US", "Iphone 15 Pro 128Gb Blue Titantium", new Guid("6d756ac2-33d8-4c21-8f31-3f4c67f50a14"), null, null, null },
                    { new Guid("32bf6033-3b09-4253-9b5a-cbe0983861a9"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Thẻ nạp Viettel 20.000đ", new Guid("42cb4a24-6bfb-4eb3-b454-8a583024cb06"), null, null, null },
                    { new Guid("335b32b1-f1dd-4cc4-90eb-ec479437857b"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Thẻ nạp Vinaphone 20.000đ", new Guid("da1ff94d-14dc-4b4a-bb81-2ff5420049a3"), null, null, null },
                    { new Guid("4600d437-0574-46b5-a79d-1264a6fb69a1"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] Viettel Mobile Card 10,000 VND", new Guid("fc29bd59-2220-4bf3-b003-5f0ca116a634"), null, null, null },
                    { new Guid("4c1e37cf-537d-4703-8ddc-b023deb45d6a"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Thẻ nạp Vietnamobile 50.000đ", new Guid("ab2e9d9b-dfc1-47fa-8f7a-1e039478a20b"), null, null, null },
                    { new Guid("5cbd05d7-1afc-4830-a404-56cde2528534"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] Viettel Mobile Card 50,000 VND", new Guid("3c641e97-1a87-4f63-b94c-08693d3d66a7"), null, null, null },
                    { new Guid("6d76a7da-0a8b-47ae-bd40-05e50712ec32"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] Mobifone Mobile Card 20,000 VND", new Guid("2b28eb9a-432d-436e-9968-42b47c89a12f"), null, null, null },
                    { new Guid("6ecb1be3-9d59-4973-8a90-ed207dbc05c1"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] Vinaphone Mobile Card 20,000 VND", new Guid("da1ff94d-14dc-4b4a-bb81-2ff5420049a3"), null, null, null },
                    { new Guid("70ca7ed8-b26b-4f6e-a8ed-70435369fffc"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Thẻ nạp Vinaphone 50.000đ", new Guid("0defe395-1de1-4371-b163-c99484ed66a4"), null, null, null },
                    { new Guid("7a56c473-7093-4aeb-8be9-67a0e0044ad1"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] Vietnamobile Mobile Card 50,000 VND", new Guid("ab2e9d9b-dfc1-47fa-8f7a-1e039478a20b"), null, null, null },
                    { new Guid("7a6c4210-ce8a-4b7e-8d86-d61e775f562a"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Thẻ nạp Mobifone 50.000đ", new Guid("50781c44-617a-49e2-a7a3-a239178495bc"), null, null, null },
                    { new Guid("84e87182-e2db-499a-a6ae-c0d7f17f8bb2"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[\"Learners who exchange FSEL Coins on the Marketplace will receive one (01) gift redemption code.\",\"Upon successful redemption, please save the code for verification purposes when requested.\",\"Gift collection timeline: Immediately after receiving the redemption code, please contact the hotline at 0906 259 442 to verify your information and claim your gift.\",\"Gift delivery method: FSEL will deliver the gift to the address provided by the learner.\"],\"conditions\":[\"The gift has no cash value\",\"Each gift redemption code corresponds to a product quantity of 1\",\"The number of gifts is limited according to the document \\\"Self-study month\\\" announced by the D0ET of NgheAn\",\"The number of gifts automatically decreases according to the number of students who have redeemed the gift\",\"Only students who purchase a course between June 1, 2025 and June 30, 2025 are eligible to receive gifts.\",\"Accounts that received a free course—such as those granted additional study time or using a 100% discount voucher—are not eligible for gift redemption.\"],\"contacts\":[\"For any questions, please contact hotline: 0906 259 442\",\"Email: support_fsel@atlantic.edu.vn\"],\"others\":null}", false, "en-US", "Laptop Apple MacBook Air 13 inch M2 8CPU/8GPU/16GB/256GB - Space Grey MC7U4SA/A - Genuine Apple Vietnam", new Guid("48f395b0-441a-4c24-aa28-de7a654c8a80"), null, null, null },
                    { new Guid("94cabc9f-31a1-4397-8d64-67481016851d"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] Viettel Mobile Card 20,000 VND", new Guid("42cb4a24-6bfb-4eb3-b454-8a583024cb06"), null, null, null },
                    { new Guid("a12c0535-aad9-47d2-8be6-44bbef8909f8"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] - Circle K 50,000 VND", new Guid("c21b61be-ce79-4c02-9fb8-54fc6d482a40"), null, null, null },
                    { new Guid("ab2edcef-8b16-4518-a943-65a49775b724"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Thẻ nạp Mobifone 20.000đ", new Guid("2b28eb9a-432d-436e-9968-42b47c89a12f"), null, null, null },
                    { new Guid("ae7f98fd-8c63-4d01-894f-7df9a496eacc"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] - Circle K 20,000 VND", new Guid("1dd2462c-22a6-4319-95b0-a1bf9f719e0d"), null, null, null },
                    { new Guid("b107c1ad-cb2e-4e6c-9e2d-e2c885dd8eac"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Thẻ nạp Vinaphone 10.000đ", new Guid("3114a892-4cef-49ec-8cd6-94785c83386f"), null, null, null },
                    { new Guid("b1a875fb-276a-4c7a-b448-b01604392d0f"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] Vietnamobile Mobile Card 20,000 VND", new Guid("27885469-9843-49f6-92ad-c5ae0d84ae71"), null, null, null },
                    { new Guid("b6f818dc-ef5a-4b7b-b5a9-8dd8a4f3ea87"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Thẻ nạp Viettel 50.000đ", new Guid("3c641e97-1a87-4f63-b94c-08693d3d66a7"), null, null, null },
                    { new Guid("c0dacf7a-906c-4c5b-8c3b-dc4e97905fe3"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[\"Learners who exchange FSEL Coins on the Marketplace will receive one (01) gift redemption code.\",\"Upon successful redemption, please save the code for verification purposes when requested.\",\"Gift collection timeline: Immediately after receiving the redemption code, please contact the hotline at 0906 259 442 to verify your information and claim your gift.\",\"Gift delivery method: FSEL will deliver the gift to the address provided by the learner.\"],\"conditions\":[\"The gift has no cash value\",\"Each gift redemption code corresponds to a product quantity of 1\",\"The number of gifts is limited according to the document \\\"Self-study month\\\" announced by the D0ET of NgheAn\",\"The number of gifts automatically decreases according to the number of students who have redeemed the gift\",\"Only students who purchase a course between June 1, 2025 and June 30, 2025 are eligible to receive gifts.\",\"Accounts that received a free course—such as those granted additional study time or using a 100% discount voucher—are not eligible for gift redemption.\"],\"contacts\":[\"For any questions, please contact hotline: 0906 259 442\",\"Email: support_fsel@atlantic.edu.vn\"],\"others\":null}", false, "en-US", "Apple Airpods 4 Headphones (Active Noise Cancellation)", new Guid("4f13c2f5-d2f7-4395-b2c7-c97e9ff9ef89"), null, null, null },
                    { new Guid("c3695183-1337-420d-87df-00ad9a8ac71e"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[\"Học viên đổi số Xu FSEL (FSEL Coin) tương ứng trên Marketplace sẽ nhận được 01 mã đổi quà.\",\"Sau khi đổi thành công vui lòng lưu lại mã quà tặng để đối chiếu khi được yêu cầu.\",\"Thời gian nhận quà: Sau ngay nhận mã đổi thưởng, vui lòng liên hệ hotline 0906259442 để xác thực thông tin và nhận quà\",\"Hình thức nhận quà: FSEL chuyển quà tới học viên qua địa chỉ học sinh cung cấp\"],\"conditions\":[\"Quà không có giá trị quy đổi thành tiền mặt\",\"Mỗi mã đổi quà tương ứng với 01 sản phẩm\",\"Số lượng quà tự động giảm dần theo số lượng học sinh đã đổi quà\",\"Chỉ những học sinh mua khóa học trong khoảng thời gian từ ngày 01/06/2025 đến hết ngày 30/06/2025 mới đủ điều kiện nhận quà.\",\"Các tài khoản được tặng khóa học miễn phí, bao gồm tài khoản được tặng thêm tháng học hoặc sử dụng voucher giảm giá 100%, sẽ không đủ điều kiện để đổi quà.\"],\"contacts\":[\"Mọi thắc mắc liên hệ hotline: 0906 259 442\",\"Email: support_fsel@atlantic.edu.vn\"],\"others\":null}", false, "vi-VN", "Laptop Apple MacBook Air 13 inch M2 8CPU/8GPU/16GB/256GB - Space Grey MC7U4SA/A - Chính Hãng Apple Việt Nam", new Guid("48f395b0-441a-4c24-aa28-de7a654c8a80"), null, null, null },
                    { new Guid("d275c7c1-7cd0-4ad9-8f7d-076be5602bb2"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] Vinaphone Mobile Card 50,000 VND", new Guid("0defe395-1de1-4371-b163-c99484ed66a4"), null, null, null },
                    { new Guid("d2adf735-ebff-4613-a703-b8655f855d96"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[\"Học viên đổi số Xu FSEL (FSEL Coin) tương ứng trên Marketplace sẽ nhận được 01 mã đổi quà.\",\"Sau khi đổi thành công vui lòng lưu lại mã quà tặng để đối chiếu khi được yêu cầu.\",\"Thời gian nhận quà: Sau ngay nhận mã đổi thưởng, vui lòng liên hệ hotline 0906259442 để xác thực thông tin và nhận quà\",\"Hình thức nhận quà: FSEL chuyển quà tới học viên qua địa chỉ học sinh cung cấp\"],\"conditions\":[\"Quà không có giá trị quy đổi thành tiền mặt\",\"Mỗi mã đổi quà tương ứng với 01 sản phẩm\",\"Số lượng quà tự động giảm dần theo số lượng học sinh đã đổi quà\",\"Chỉ những học sinh mua khóa học trong khoảng thời gian từ ngày 01/06/2025 đến hết ngày 30/06/2025 mới đủ điều kiện nhận quà.\",\"Các tài khoản được tặng khóa học miễn phí, bao gồm tài khoản được tặng thêm tháng học hoặc sử dụng voucher giảm giá 100%, sẽ không đủ điều kiện để đổi quà.\"],\"contacts\":[\"Mọi thắc mắc liên hệ hotline: 0906 259 442\",\"Email: support_fsel@atlantic.edu.vn\"],\"others\":null}", false, "vi-VN", "Tai nghe Apple Airpods 4 (Chống ồn chủ động)", new Guid("4f13c2f5-d2f7-4395-b2c7-c97e9ff9ef89"), null, null, null },
                    { new Guid("d346e51b-a2be-4ce2-873b-d5eec47ff149"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Thẻ nạp Viettel 10.000đ", new Guid("fc29bd59-2220-4bf3-b003-5f0ca116a634"), null, null, null },
                    { new Guid("ddc57211-e5fe-47ca-887f-c9c5293100e5"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "en-US", "[UrBox Voucher] Mobifone Mobile Card 50,000 VND", new Guid("50781c44-617a-49e2-a7a3-a239178495bc"), null, null, null },
                    { new Guid("e8560c0b-8e91-4676-a936-f9a24b9c9052"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Circle K 50,000 VND", new Guid("c21b61be-ce79-4c02-9fb8-54fc6d482a40"), null, null, null },
                    { new Guid("ebf3dd3e-9f3d-4a48-b639-3c90434c91fc"), new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, "{\"howToUses\":[],\"conditions\":[],\"contacts\":[],\"others\":null}", false, "vi-VN", "[UrBox Voucher] Circle K 20,000 VND", new Guid("1dd2462c-22a6-4319-95b0-a1bf9f719e0d"), null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("001ec6cc-b059-403f-be40-da89a476732b"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("0dac99b5-4c11-44d7-98a5-2fd5fcff06e8"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("22fd9daa-d9ec-48c9-b009-de390cc7efd3"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("29e66472-b20e-4b1f-aa7d-c2bd2b2c0f74"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("2d19bc52-35ef-44a7-b6bc-6fa45b99c0cc"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("2ed60c84-7245-4e62-a328-ca6055085a70"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("32bf6033-3b09-4253-9b5a-cbe0983861a9"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("335b32b1-f1dd-4cc4-90eb-ec479437857b"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4600d437-0574-46b5-a79d-1264a6fb69a1"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("4c1e37cf-537d-4703-8ddc-b023deb45d6a"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("5cbd05d7-1afc-4830-a404-56cde2528534"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6d76a7da-0a8b-47ae-bd40-05e50712ec32"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("6ecb1be3-9d59-4973-8a90-ed207dbc05c1"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("70ca7ed8-b26b-4f6e-a8ed-70435369fffc"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7a56c473-7093-4aeb-8be9-67a0e0044ad1"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("7a6c4210-ce8a-4b7e-8d86-d61e775f562a"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("84e87182-e2db-499a-a6ae-c0d7f17f8bb2"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("94cabc9f-31a1-4397-8d64-67481016851d"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("a12c0535-aad9-47d2-8be6-44bbef8909f8"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ab2edcef-8b16-4518-a943-65a49775b724"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ae7f98fd-8c63-4d01-894f-7df9a496eacc"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b107c1ad-cb2e-4e6c-9e2d-e2c885dd8eac"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b1a875fb-276a-4c7a-b448-b01604392d0f"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("b6f818dc-ef5a-4b7b-b5a9-8dd8a4f3ea87"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("c0dacf7a-906c-4c5b-8c3b-dc4e97905fe3"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("c3695183-1337-420d-87df-00ad9a8ac71e"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d275c7c1-7cd0-4ad9-8f7d-076be5602bb2"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d2adf735-ebff-4613-a703-b8655f855d96"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("d346e51b-a2be-4ce2-873b-d5eec47ff149"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ddc57211-e5fe-47ca-887f-c9c5293100e5"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("e8560c0b-8e91-4676-a936-f9a24b9c9052"));

            migrationBuilder.DeleteData(
                table: "ProductTranslations",
                keyColumn: "Id",
                keyValue: new Guid("ebf3dd3e-9f3d-4a48-b639-3c90434c91fc"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("0defe395-1de1-4371-b163-c99484ed66a4"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("1dd2462c-22a6-4319-95b0-a1bf9f719e0d"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("27885469-9843-49f6-92ad-c5ae0d84ae71"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("2b28eb9a-432d-436e-9968-42b47c89a12f"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("3114a892-4cef-49ec-8cd6-94785c83386f"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("3c641e97-1a87-4f63-b94c-08693d3d66a7"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("42cb4a24-6bfb-4eb3-b454-8a583024cb06"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("48f395b0-441a-4c24-aa28-de7a654c8a80"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("4f13c2f5-d2f7-4395-b2c7-c97e9ff9ef89"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("50781c44-617a-49e2-a7a3-a239178495bc"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("6d756ac2-33d8-4c21-8f31-3f4c67f50a14"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("9d449591-6cc6-4e2f-b75c-521a7c68edb9"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ab2e9d9b-dfc1-47fa-8f7a-1e039478a20b"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c21b61be-ce79-4c02-9fb8-54fc6d482a40"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("da1ff94d-14dc-4b4a-bb81-2ff5420049a3"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("fc29bd59-2220-4bf3-b003-5f0ca116a634"));

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsPremium",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductGlobalConfigStr",
                table: "Products");
        }
    }
}
