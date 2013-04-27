(function() {
  jQuery(function() {
    var $root, Activity, activity;

    $root = $('section#activity');
    if ($root.length > 0) {
      $root.delegate('button#activity-load-more', 'click', function(e) {
        e.preventDefault();
        activity.loadModeItems();
        return false;
      });
      Activity = (function() {
        function Activity() {
          this.nextPage = 2;
          this.table = $root.find('table.history tbody');
        }

        Activity.prototype.loadModeItems = function() {
          $.ajax({
            url: "/dashboard/getnextactivitypage?pagenumber=" + activity.nextPage,
            success: function(data) {
              activity.nextPage++;
              return activity.table.append(data, {});
            },
            error: function(e) {
              console.log(e);
              return Errordite.Alert.show('Something went wrong getting the next page, please try again.');
            }
          });
          return true;
        };

        return Activity;

      })();
      activity = new Activity();
      return true;
    }
  });

}).call(this);
