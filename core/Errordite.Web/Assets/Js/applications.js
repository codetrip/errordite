(function() {

  jQuery(function() {
    var $body, Application, application;
    $body = $('section#applications');
    if ($body.length > 0) {
      application = null;
      $body.delegate('a.delete-application', 'click', function(e) {
        var $this;
        $this = $(this);
        application = new Application($this.closest('tr'));
        application["delete"]();
        return e.preventDefault();
      });
      $body.delegate('a.generate-error', 'click', function(e) {
        var $this;
        $this = $(this);
        new Application($this.closest('tr')).generateError();
        return e.preventDefault();
      });
      return Application = (function() {

        function Application($appEl) {
          this.$appEl = $appEl;
        }

        Application.prototype["delete"] = function() {
          var $element;
          $element = this.$appEl;
          return Errordite.Confirm.show("Are you sure you want to delete the selected application?", {
            okCallBack: function() {
              return $element.find('form:has(.delete-application)').submit();
            }
          });
        };

        Application.prototype.generateError = function() {
          return this.$appEl.find('form:has(.generate-error)').submit();
        };

        return Application;

      })();
    }
  });

}).call(this);
