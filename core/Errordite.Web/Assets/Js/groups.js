(function() {

  jQuery(function() {
    var $body, Group, group;
    $body = $('section#groups');
    if ($body.length > 0) {
      group = null;
      $body.delegate('a.delete', 'click', function() {
        var $this;
        $this = $(this);
        this.group = new Group($this.closest('form'));
        this.group["delete"]();
        return false;
      });
      return Group = (function() {

        function Group($form) {
          this.$form = $form;
        }

        Group.prototype["delete"] = function() {
          if (window.confirm("Are you sure you want to delete this group?")) {
            return this.$form.submit();
          }
        };

        return Group;

      })();
    }
  });

}).call(this);
