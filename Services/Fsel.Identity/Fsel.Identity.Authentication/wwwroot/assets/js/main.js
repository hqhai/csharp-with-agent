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
  });

}(jQuery));	
