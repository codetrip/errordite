(function() {

  jQuery(function() {
    var $body, Group, group;
    $body = $('section#groups');
    if ($body.length > 0) {
      group = null;
      $body.delegate('a.delete', 'click', function() {
        var $this;
        $this = $(this);
        this.group = new Group($('form#deleteGroup'), $this.data('val'));
        this.group["delete"]();
        return false;
      });
      return Group = (function() {

        function Group($form, groupId) {
          this.$form = $form;
          this.groupId = groupId;
        }

        Group.prototype["delete"] = function() {
          var $form, groupId;
          $form = this.$form;
          groupId = this.groupId;
          if (window.confirm("Are you sure you want to delete this group? " + groupId)) {
            $('input#GroupId').val(groupId);
            return $form.submit();
          }
        };

        return Group;

      })();
    }
  });

}).call(this);
