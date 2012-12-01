(function() {

  jQuery(function() {
    var $body, User, user;
    $body = $('section#users');
    if ($body.length > 0) {
      user = null;
      $body.delegate('a.delete', 'click', function() {
        var $this;
        $this = $(this);
        this.user = new User($this.closest('tr'));
        this.user["delete"]();
        return false;
      });
      return User = (function() {

        function User($appEl) {
          this.$appEl = $appEl;
        }

        User.prototype["delete"] = function() {
          var $appEl;
          $appEl = this.$appEl;
          if (window.confirm("Are you sure you want to delete this user, any issues assigned to this user will be assigned to you!")) {
            return $appEl.prev('form').submit();
          }
        };

        return User;

      })();
    }
  });

}).call(this);
