(function() {
  var Dashboard, dashboard, timeout;

  (function($) {
    var $root, Dashboard, currentEndpoint, currentQueue, dashboard, timeout;
    $root = $("div#system-status");
    Dashboard = void 0;
    dashboard = void 0;
    currentEndpoint = void 0;
    currentQueue = void 0;
    timeout = void 0;
    if ($root.length > 0) {
      $root.delegate("a.purge", "click", function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        if (!confirm("Are you sure you want to delete the selected messages?")) {
          return false;
        }
        return dashboard.deleteMessages($this.data("queue"), null, $this.data("servicename"));
      });
      return $root.delegate("a.retry", "click", function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        if (!confirm("Are you sure you want to retry the selected messages?")) {
          return false;
        }
        return dashboard.returnToSource($this.data("queue"), null, $this.data("servicename"));
      });
    }
  }, $root.delegate("select#RavenInstanceId", "change", function(e) {
    var $this;
    e.preventDefault();
    $this = $(this);
    return dashboard.switchInstance($this.val());
  }), Dashboard = (function() {
    Dashboard = function() {
      return this.container = $("div.service-info-container");
    };
    Dashboard.prototype.bindEvents = function() {
      return $("th :checkbox").on("click", function() {
        return $(this).closest("table").find("td :checkbox").prop("checked", $(this).is(":checked"));
      });
    };
    Dashboard.prototype.poll = function() {
      $.ajax({
        url: "/systemadmin/styles/admin/updatesystemstatus",
        success: function(result) {
          console.log("success");
          if (result.Success) {
            return dashboard.bind(result.Data);
          }
          return false;
        },
        error: function() {
          return false;
        },
        dataType: "json"
      });
      return true;
    };
    Dashboard.prototype.deleteMessages = function(queueName, serviceName) {
      $.ajax({
        url: "/system/services/deletemessages",
        data: {
          instanceId: $('select#RavenInstanceId').val(),
          queueName: queueName,
          serviceName: serviceName
        },
        success: function(result) {
          if (result.Success) {
            return dashboard.poll();
          } else {
            alert("Failed to delete messages, please try again.");
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
    Dashboard.prototype.returnToSource = function(queueName, serviceName) {
      dashboard.pollingEnabled = false;
      $.ajax({
        url: "/system/services/returntosource",
        data: {
          instanceId: $('select#RavenInstanceId').val(),
          queueName: queueName,
          serviceName: serviceName
        },
        success: function(result) {
          if (result.Success) {
            return dashboard.poll();
          } else {
            alert("Failed to return error messages to their source queue, please try again.");
          }
          return true;
        },
        error: function() {
          dashboard.pollingEnabled = true;
          return alert("Failed to return error messages to their source queue, please try again.");
        },
        dataType: "json",
        type: "POST"
      });
      return true;
    };
    Dashboard.prototype.swapInstance = function(instanceId) {
      window.location = "/system/services?ravenInstanceId=" + instanceId;
      return true;
    };
    Dashboard.prototype.reload = function() {
      window.location = "/system/services?ravenInstanceId=" + $('select#RavenInstanceId').val();
      return true;
    };
    Dashboard.prototype.bind = function(data) {
      dashboard.container.hide("drop", {
        complete: function() {
          dashboard.container.html(data);
          return dashboard.container.show("slow");
        }
      });
      return true;
    };
    return Dashboard;
  })(), dashboard = new Dashboard(), timeout = setTimeout(dashboard.poll, 15000))(jQuery);

}).call(this);
