// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService
{
    using Fsel.Common.ActionResults;
    using Fsel.Identity.Application.Services.OrderService.CommandModels;
    using Fsel.Identity.Application.Services.OrderService.Model;
    using Fsel.Identity.Application.Services.OrderService.QueryModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.CampusModel;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IOrderService
    {
        [Get("/v1/package")]
        Task<IApiResponse<MethodResult<List<PackageModel>>>> GetPackages();

        [Post("/v1/user-referral")]
        Task<IApiResponse<MethodResult<bool>>> CreateUserReferralAsync([Body] CreateUserReferralCommandModel command);

        [Get("/v1/order/get-status")]
        Task<IApiResponse<MethodResult<EnumOrderStatus?>>> GetStatusAsync([FromQuery] GetStatusByUserCommandModel query);

        [Post("/v1/order")]
        Task<IApiResponse<MethodResult<OrderModel>>> CreateOrder([Body] CreateOrderCommandModel command);

        [Put("/v1/admin/order/change-status-order")]
        Task<IApiResponse<MethodResult<bool>>> ChangeStatusOrder([Body] ChangeStatusOrderCommandModel command);

        [Post("/v1/admin/order/create-order-for-student")]
        Task<IApiResponse<MethodResult<OrderModel>>> CreateOrderForStudentAsync([Body] CreateOrderByUserIdCommandModel command);

        [Get("/v1/order/get-order-by-status")]
        Task<IApiResponse<MethodResult<IList<OrderSearchModel>>>> GetOrderByStatusAsync([FromQuery] GetOrderByStatusQueryModel query);

        [Post("/v1.1/order/create-orders-from-crm")]
        Task<IApiResponse<MethodResult<bool>>> CreateOrdersFromCRM([Body] CreateOrdersFromCRMModels command);

        [Delete("/v1/admin/order/delete-student/{id}")]
        Task<IApiResponse<MethodResult<bool>>> DeleteListDataUser([FromRoute] Guid id);

        [Get("/v1.2/order/get-orders-by-user-id")]
        Task<IApiResponse<MethodResult<IList<OrderModel>>>> GetOrdersByUserId([Query] GetOrdersByUserIdQueryModel query);

        [Post("/v1.2/admin/order/create-order-payment")]
        Task<IApiResponse<MethodResult<bool>>> CreateOrderPayment([Body] CreateOrderPaymentCommandModel command);

        [Post("/v1.2/order/create-order-for-student-leader-board")]
        Task<IApiResponse<MethodResult<VoidMethodResult>>> CreateOrderForUserLeaderBoard([Body] CreateOrderForUserFromLeaderBoardCommandModel model);

        [Post("/v1.2/admin/order/create-order-for-students-event")]
        Task<IApiResponse<MethodResult<bool>>> CreateOrderForStudentsEvent([Body] CreateOrderForStudentsEventCommandModels models);

        [Post("/v1.2/admin/order/delete-order-of-students-event")]
        Task<IApiResponse<MethodResult<bool>>> DeleteOrderOfStudentEvent([Body] DeleteOrderOfStudentsInEventCommandModel model);

        [Post("/v1/order/get-orders-blind-bag-event")]
        Task<IApiResponse<MethodResult<IList<OrderModel>>>> GetRecentOrdersAsync([Body] GetRecentOrdersToUserIdsQueryModel query);

        [Post("/v1.2/admin/order/get-orders-by-user-ids")]
        Task<IApiResponse<MethodResult<OrdersByUserIdsModels>>> GetOrdersByUserIds([Body] GetOrdersByUserIdsQueryModel model);

        [Post("/v1/admin/order/create-order-students-event")]
        Task<IApiResponse<MethodResult<bool>>> CreateOrderEventByStudent([Body] CreateOrderForStudentEventCommandModel command);

        [Post("/v1.2/admin/order/get-users-has-order-revenue")]
        Task<IApiResponse<MethodResult<IList<OrderModel>>>> GetUserHasOrderRevenue([Body] GetUserHasOrderRevenueModel query);

        [Post("/v1/campus/create-order-for-students-campus")]
        Task<IApiResponse<MethodResult<bool>>> CreateOrderForStudentCampus([Body] CreateOrdersForStudentCampusCommandModels command);

        [Post("/v1/campus/delete-order-of-students-campus")]
        Task<IApiResponse<MethodResult<bool>>> DeleteOrderOfStudentsCampus([Body] DeleteOrderOfStudentsCampusCommandModel command);
    }
}
