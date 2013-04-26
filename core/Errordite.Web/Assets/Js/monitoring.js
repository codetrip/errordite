(function() {
  jQuery(function() {
    var $root, Monitoring, monitoring;

    $root = $("section#monitoring");
    if ($root.length > 0) {
      $('th :checkbox').on('click', function() {
        $(this).closest('table').find('td :checkbox').prop('checked', $(this).is(':checked'));
        return monitoring.maybeEnableActions();
      });
      $root.delegate("a.purge", "click", function(e) {
        var $this;

        e.preventDefault();
        $this = $(this);
        if (!confirm("Are you sure you want to delete all messages?")) {
          return false;
        }
        return serviceManager.deleteMessages($this.data("service"));
      });
      $root.delegate("a.retry", "click", function(e) {
        var $this;

        e.preventDefault();
        $this = $(this);
        if (!confirm("Are you sure you want to retry all messages?")) {
          return false;
        }
        return serviceManager.returnToSource($this.data("service"));
      });
      Monitoring = (function() {
        function Monitoring() {}

        Monitoring.prototype.deleteMessages = function() {
          var deleteMessages;

          deleteMessages = function(serviceName) {};
          $.ajax({
            url: "/system/services/deletemessages",
            data: {
              instanceId: $('select#RavenInstanceId').val(),
              serviceName: serviceName
            },
            success: function(result) {
              if (result.success) {
                location.reload();
              } else {
                return Errordite.Alert.show("Failed to delete messages, please try again.");
              }
              return true;
            },
            error: function() {
              return alert("Failed to delete messages, please try again.");
            },
            dataType: "json",
            type: "POST"
          });
          return true;
        };

        Monitoring.prototype.retryMessages = function(serviceName) {
          $.ajax({
            url: "/system/services/returntosource",
            data: {
              instanceId: $('select#RavenInstanceId').val(),
              serviceName: serviceName
            },
            success: function(result) {
              if (result.success) {
                location.reload();
              } else {
                Errordite.Alert.show("Failed to return error messages to their source queue, please try again.");
              }
              return true;
            },
            error: function() {
              return Errordite.Alert.show("Failed to return error messages to their source queue, please try again.");
            },
            dataType: "json",
            type: "POST"
          });
          return true;
        };

        Monitoring.prototype.maybeEnableActions = function() {
          $('ul#action-list').toggle(!!$(':checkbox:checked[name=envelopes]').length);
          return true;
        };

        return Monitoring;

      })();
      monitoring = new Monitoring();
      monitoring.maybeEnableActions();
      return true;
    }
  });

}).call(this);
