(function ($) {
  $("#languageContainer").on("click", function () {
    if ($(this).hasClass('show')) {
      $(this).removeClass("show");
    }
    else {
      $(this).addClass("show");
    }
  });

}(jQuery));	
