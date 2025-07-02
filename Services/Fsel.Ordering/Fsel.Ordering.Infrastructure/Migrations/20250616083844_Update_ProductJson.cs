using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fsel.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update_ProductJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("0defe395-1de1-4371-b163-c99484ed66a4"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Vinaphone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721341_5e842ffdee2fc.png\",\"content\":null,\"note\":null,\"addresses\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("1dd2462c-22a6-4319-95b0-a1bf9f719e0d"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Circle K\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585713650_5e8411f295a7b.png\",\"content\":null,\"note\":null,\"addresses\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("27885469-9843-49f6-92ad-c5ae0d84ae71"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Vietnamobile\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721231_5e842f8f7bde0.png\",\"content\":null,\"note\":null,\"addresses\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("2b28eb9a-432d-436e-9968-42b47c89a12f"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Mobifone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585718650_5e84257b025ac.png\",\"content\":null,\"note\":null,\"addresses\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("3114a892-4cef-49ec-8cd6-94785c83386f"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Vinaphone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721341_5e842ffdee2fc.png\",\"content\":null,\"note\":null,\"addresses\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("3c641e97-1a87-4f63-b94c-08693d3d66a7"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Viettel\",\"brandImage\":\"https://images.urbox.vn/_img_server/2021/07/16/160/1626425459_60f14873d5d12.png\",\"content\":null,\"note\":null,\"addresses\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("42cb4a24-6bfb-4eb3-b454-8a583024cb06"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Viettel\",\"brandImage\":\"https://images.urbox.vn/_img_server/2021/07/16/160/1626425459_60f14873d5d12.png\",\"content\":null,\"note\":null,\"addresses\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("48f395b0-441a-4c24-aa28-de7a654c8a80"),
                column: "ImagesStr",
                value: "[\"https://genk.mediacdn.vn/139269124445442048/2022/7/16/macbook-air-m2-chip-purple-feature-1657942675923-1657942676585833475599.jpg\"]");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("4f13c2f5-d2f7-4395-b2c7-c97e9ff9ef89"),
                column: "ImagesStr",
                value: "[\"https://m-cdn.phonearena.com/images/reviews/263078-image/BK6A7427.jpg?w=1\"]");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("50781c44-617a-49e2-a7a3-a239178495bc"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Mobifone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585718650_5e84257b025ac.png\",\"content\":null,\"note\":null,\"addresses\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("9d449591-6cc6-4e2f-b75c-521a7c68edb9"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Mobifone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585718650_5e84257b025ac.png\",\"content\":null,\"note\":null,\"addresses\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ab2e9d9b-dfc1-47fa-8f7a-1e039478a20b"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Vietnamobile\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721231_5e842f8f7bde0.png\",\"content\":null,\"note\":null,\"addresses\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c21b61be-ce79-4c02-9fb8-54fc6d482a40"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Circle K\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585713650_5e8411f295a7b.png\",\"content\":null,\"note\":null,\"addresses\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("da1ff94d-14dc-4b4a-bb81-2ff5420049a3"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Vinaphone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721341_5e842ffdee2fc.png\",\"content\":null,\"note\":null,\"addresses\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("fc29bd59-2220-4bf3-b003-5f0ca116a634"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Viettel\",\"brandImage\":\"https://images.urbox.vn/_img_server/2021/07/16/160/1626425459_60f14873d5d12.png\",\"content\":null,\"note\":null,\"addresses\":null}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("0defe395-1de1-4371-b163-c99484ed66a4"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Vinaphone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721341_5e842ffdee2fc.png\",\"content\":null,\"note\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("1dd2462c-22a6-4319-95b0-a1bf9f719e0d"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Circle K\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585713650_5e8411f295a7b.png\",\"content\":null,\"note\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("27885469-9843-49f6-92ad-c5ae0d84ae71"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Vietnamobile\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721231_5e842f8f7bde0.png\",\"content\":null,\"note\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("2b28eb9a-432d-436e-9968-42b47c89a12f"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Mobifone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585718650_5e84257b025ac.png\",\"content\":null,\"note\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("3114a892-4cef-49ec-8cd6-94785c83386f"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Vinaphone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721341_5e842ffdee2fc.png\",\"content\":null,\"note\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("3c641e97-1a87-4f63-b94c-08693d3d66a7"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Viettel\",\"brandImage\":\"https://images.urbox.vn/_img_server/2021/07/16/160/1626425459_60f14873d5d12.png\",\"content\":null,\"note\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("42cb4a24-6bfb-4eb3-b454-8a583024cb06"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Viettel\",\"brandImage\":\"https://images.urbox.vn/_img_server/2021/07/16/160/1626425459_60f14873d5d12.png\",\"content\":null,\"note\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("48f395b0-441a-4c24-aa28-de7a654c8a80"),
                column: "ImagesStr",
                value: "[\"https://anphat.com.vn/media/product/50827_laptop_apple_macbook_air_13_inch_m2_8cpu8gpu16gb256gb___space_grey_mc7u4saa__1_.jpg\",\"https://anphat.com.vn/media/product/50827_laptop_apple_macbook_air_13_inch_m2_8cpu8gpu16gb256gb___space_grey_mc7u4saa__4_.jpg\",\"https://anphat.com.vn/media/product/50827_laptop_apple_macbook_air_13_inch_m2_8cpu8gpu16gb256gb___space_grey_mc7u4saa__3_.jpg\"]");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("4f13c2f5-d2f7-4395-b2c7-c97e9ff9ef89"),
                column: "ImagesStr",
                value: "[\"https://www.maccenter.vn/Accessories/Apple-AirPods4-A.jpg\",\"https://cdn.viettelstore.vn/Images/Product/ProductImage/479432236.jpeg\",\"https://cdn2.cellphones.com.vn/x/media/catalog/product/a/i/airpods-4-chong-on-7.png\"]");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("50781c44-617a-49e2-a7a3-a239178495bc"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Mobifone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585718650_5e84257b025ac.png\",\"content\":null,\"note\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("9d449591-6cc6-4e2f-b75c-521a7c68edb9"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Mobifone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585718650_5e84257b025ac.png\",\"content\":null,\"note\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("ab2e9d9b-dfc1-47fa-8f7a-1e039478a20b"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Vietnamobile\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721231_5e842f8f7bde0.png\",\"content\":null,\"note\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("c21b61be-ce79-4c02-9fb8-54fc6d482a40"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Circle K\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585713650_5e8411f295a7b.png\",\"content\":null,\"note\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("da1ff94d-14dc-4b4a-bb81-2ff5420049a3"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Vinaphone\",\"brandImage\":\"https://images.urbox.vn/_img_server/2020/04/01/160/1585721341_5e842ffdee2fc.png\",\"content\":null,\"note\":null}");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("fc29bd59-2220-4bf3-b003-5f0ca116a634"),
                column: "ProductGlobalConfigStr",
                value: "{\"brandName\":\"Viettel\",\"brandImage\":\"https://images.urbox.vn/_img_server/2021/07/16/160/1626425459_60f14873d5d12.png\",\"content\":null,\"note\":null}");
        }
    }
}
