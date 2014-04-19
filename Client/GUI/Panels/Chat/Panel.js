// Generated by CoffeeScript 1.7.1
(function() {
  $("#chat").dialog({
    dialogClass: "ui-noclose",
    closeOnEscape: false,
    draggable: false,
    resizable: false,
    width: 400,
    height: "auto",
    show: {
      effect: "fadeIn",
      duration: 300
    }
  });

  $("#chat").parent().css({
    position: "fixed",
    top: "",
    left: 6,
    bottom: 6
  });

  $("#chat").parent().hover((function() {
    return $(this).fadeTo(200, 1.0);
  }), (function() {
    return $(this).fadeTo(200, 0.8);
  }));

}).call(this);

//# sourceMappingURL=Panel.map
