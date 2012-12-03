(function() {

  jQuery(function() {
    var $alerts, $body, Alerts, alertHeaderHeight, alertHeight;
    $body = $('div#alerts');
    $alerts = null;
    alertHeight = 58;
    alertHeaderHeight = 46;
    if ($body.length > 0) {
      $("div#head-links").delegate('a#show-alerts', 'click', function(e) {
        e.preventDefault();
        if ($body.find('div.alert').length === 0) {
          $alerts = new Alerts();
          return $alerts.show(function() {
            if ($body.find('div.alert').length === 0) {
              $alerts.setTimeout();
              return alert("You have no alerts at present");
            }
          });
        } else {
          $body.show();
          return $body.animate({
            height: ($body.find('div.alert').length * alertHeight) + alertHeaderHeight
          }, 1000);
        }
      });
      $(document).ready(function() {
        $alerts = new Alerts();
        if (($.cookie("alerts") === null || $.cookie("alerts") === "show") && $alerts.hasTimedOut()) {
          $alerts.show();
          return $alerts.setTimeout();
        }
      });
      $body.delegate('a#hidealerts', 'click', function(e) {
        $alerts.hide();
        return false;
      });
      $body.delegate('a#dismissalerts', 'click', function(e) {
        Errordite.Spinner.disable();
        $.post('/alerts/dismissall');
        Errordite.Spinner.enable();
        $alerts.setTimeout();
        return false;
      });
      return Alerts = (function() {

        Alerts.name = 'Alerts';

        function Alerts() {}

        Alerts.prototype.bind = function(alerts) {
          var a, _i, _len, _ref;
          $('div').remove('.alert');
          _ref = alerts.data;
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            a = _ref[_i];
            $body.append('<div class="alert alert-success" data-alert-id="' + a.Id + '" data-alert-utc="' + a.Date + '"><a class="close" data-dismiss="alert">X</a><h4 class="alert-heading">' + a.Header + ' on ' + a.Date + '</h4>' + a.Message + '</div>');
          }
          $body.css({
            left: ($(window).width() / 2) - 300,
            display: 'block'
          });
          $body.animate({
            height: (alerts.data.length * alertHeight) + alertHeaderHeight
          }, 500);
          return $('.alert').bind('closed', function() {
            var $this, alertCount;
            $this = $(this);
            alertCount = $body.find('div.alert').length;
            if (alertCount <= 3) {
              if (alertCount === 1) {
                $body.animate({
                  height: 0
                }, 500, function() {
                  return $body.hide();
                });
              } else {
                $body.animate({
                  height: ((alertCount - 1) * alertHeight) + alertHeaderHeight - 5
                }, 500);
              }
            }
            Errordite.Spinner.disable();
            $.post('/alerts/dismiss', {
              id: $this.closest('[data-alert-id]').data('alert-id')
            });
            return Errordite.Spinner.enable();
          });
        };

        Alerts.prototype.show = function(complete) {
          this.setCookie("show");
          Errordite.Spinner.disable();
          $.get('/alerts/get', function(alerts) {
            if (alerts.data.length > 0) {
              $alerts.bind(alerts);
            }
            if (complete != null) {
              return complete();
            }
          });
          return Errordite.Spinner.enable();
        };

        Alerts.prototype.hide = function() {
          this.setCookie("hide");
          return $body.animate({
            height: 0
          }, 500, function() {
            return $body.hide();
          });
        };

        Alerts.prototype.setCookie = function(val) {
          var expiry;
          expiry = new Date();
          expiry.setMinutes(expiry.getMinutes() + 60);
          return $.cookie("alerts", val, {
            expires: expiry
          });
        };

        Alerts.prototype.setTimeout = function() {
          var expiry;
          expiry = new Date();
          expiry.setMinutes(expiry.getMinutes() + 5);
          return $.cookie("alerts-timeout", "", {
            expires: expiry
          });
        };

        Alerts.prototype.hasTimedOut = function() {
          return $.cookie("alerts-timeout") === null;
        };

        return Alerts;

      })();
    }
  });

}).call(this);
