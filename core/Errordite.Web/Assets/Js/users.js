(function() {

  jQuery(function() {
    var $body, User, user;
    $body = $('section#users');
    if ($body.length > 0) {
      user = null;
      $body.delegate('a.delete', 'click', function() {
        var $this;
        $this = $(this);
        this.user = new User($this.closest('form'));
        this.user["delete"]();
        return false;
      });
      $body.delegate('a.invite', 'click', function() {
        var $this;
        $this = $(this);
        $this.closest('form').submit();
        return false;
      });
      return User = (function() {

        function User($form) {
          this.$form = $form;
        }

        User.prototype["delete"] = function() {
          if (window.confirm("Are you sure you want to delete this user, any issues assigned to this user will be assigned to you!")) {
            return this.$form.submit();
          }
        };

        return User;

      })();
    }
  });

}).call(this);
