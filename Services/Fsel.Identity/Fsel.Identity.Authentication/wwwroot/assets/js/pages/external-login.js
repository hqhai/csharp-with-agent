(function ($) {
  //$('input[name="Birthday"]').daterangepicker({
  //  singleDatePicker: true,
  //  showDropdowns: true,
  //  minYear: 1901,
  //  maxDate: moment(), 
  //  opens: "left",
  //  buttonClasses: "btn-sm",
  //})
  //.on("apply.daterangepicker", function (ev, picker) {
  //  var selectedDate = picker.startDate.format("YYYY-MM-DD");
  //  if (!picker.startDate.isValid() || !picker.endDate.isValid()) {
  //    $(this).val('')
  //  }
  //});

  flatpickr("#Birthday", {
    position: "right",

    // Tuỳ chọn khác của datepicker
  });
  
  $(document).on("click", ".available", function () {
    $('input[name="Birthday"]').data("daterangepicker").hide();
    $('input[name="Birthday"]').trigger("apply.daterangepicker");
  });

}(jQuery));	
