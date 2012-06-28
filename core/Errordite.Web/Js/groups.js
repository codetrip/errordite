(function() {
  jQuery(function() {
    var $body, Group, group;
    $body = $('div#groups');
    if ($body.length > 0) {
      group = null;
      $body.delegate('a.delete', 'click', function() {
        var $this;
        $this = $(this);
        this.group = new Group($this.closest('tr'));
        this.group["delete"]();
        return false;
      });
      return Group = (function() {
        function Group($appEl) {
          this.$appEl = $appEl;
        }
        Group.prototype["delete"] = function() {
          var $appEl;
          $appEl = this.$appEl;
          if (window.confirm("Are you sure you want to delete this group?")) {
            return $appEl.prev('form').submit();
          }
        };
        return Group;
      })();
    }
  });
}).call(this);
