(function() {

  jQuery(function() {
    var $root;
    $root = $('section#test');
    if ($root.length > 0) {
      return $root.delegate('select#ErrorId', 'change', function() {
        var $this;
        $this = $(this);
        $.ajax({
          url: "/test/getjson?errorId=" + $this.val() + '&token=' + $("select#Token").val(),
          success: function(data) {
            return $("#Json").val(data);
          },
          error: function(e) {
            console.log(e);
            return Errordite.Alert.show('Something went wrong getting the error template, please try again.');
          }
        });
        return false;
      });
    }
  });

}).call(this);
