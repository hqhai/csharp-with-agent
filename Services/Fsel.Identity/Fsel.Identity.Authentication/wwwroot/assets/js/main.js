(function ($) {
  $("#languageContainer").on("click", function () {
    if ($(this).hasClass('show')) {
      $(this).removeClass("show");
    }
    else {
      $(this).addClass("show");
    }
  });

  $("#languageDropdown").on("click", ".language-dropdown-item", function () {
    const currentHtml = $(this).html();
    const existingHtml = $("#languageContainer .language").html();

    $(this).html(existingHtml);
    $("#languageContainer .language").html(currentHtml);

    var url = $(this).attr("data-href");
    var returnUrl = encodeURIComponent(window.location.href);
    var newUrl = `${url}&returnUrl=${returnUrl}`;
    window.location.href = newUrl;
  });

  setTimeout(function () {
    $(".message-error").hide();
  }, 5000);

  $('.validation-message-text').each(function () {
    var field = $(this).attr('data-field');
    var validate = $(this).attr('data-validate');
    if (field) {
      $(`[name="${field}"]`).attr(validate, $(this).html());
    }
  });

}(jQuery));	
