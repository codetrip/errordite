(function() {
  jQuery(function() {
    var $cacheroot, $orgroot;

    $orgroot = $('section#organisations');
    $cacheroot = $('section#caching');
    if ($cacheroot.length > 0) {
      $cacheroot.delegate("select#CacheEngine", "change", function() {
        var $this, index;

        $this = $(this);
        index = window.location.href.indexOf("?");
        if (index === -1) {
          return window.location = window.location.href + "?engine=" + $this.val();
        } else {
          return window.location = window.location.href.substring(0, index) + "?engine=" + $this.val();
        }
      });
    }
    if ($orgroot.length > 0) {
      $orgroot.delegate('form#suspendForm', 'submit', function(e) {
        var $this;

        e.preventDefault();
        $this = $(this);
        return $.post($this.attr('action'), $this.serialize(), function(data) {
          return window.location.reload();
        });
      });
      $orgroot.delegate('a.suspend', 'click', function(e) {
        var $modal, $this;

        e.preventDefault();
        $this = $(this);
        $modal = $orgroot.find('div#suspend-modal');
        if ($modal === null) {
          return null;
        }
        $modal.find('input[type=hidden]').val($this.data('val'));
        return $modal.modal();
      });
      $orgroot.delegate('input[type=submit].activate', 'click', function(e) {
        var $this;

        $this = $(this);
        if (confirm("are you sure you want to activate this organisation?")) {
          return true;
        }
        e.preventDefault();
        return false;
      });
      $orgroot.delegate('input[type=submit].delete', 'click', function(e) {
        var $this;

        $this = $(this);
        if (confirm("are you sure you want to delete this organisation, all data will be permenantly deleted?")) {
          return true;
        }
        e.preventDefault();
        return false;
      });
      return $orgroot.delegate('a.stats', 'click', function(e) {
        var $modal, $this;

        e.preventDefault();
        $this = $(this);
        $modal = $orgroot.find('div#stats-modal');
        if ($modal === null) {
          return null;
        }
        $.ajax({
          url: "/system/organisations/stats?organisationId=" + $this.data('orgid'),
          success: function(result) {
            var $table;

            if (result.success) {
              $table = $modal.find('table#stats');
              $table.empty();
              $table.append('<tr><td>Issues</td><td>' + result.data.Issues + '</td></tr>');
              $table.append('<tr><td>Acknowledged</td><td>' + result.data.Acknowledged + '</td></tr>');
              $table.append('<tr><td>Unacknowledged</td><td>' + result.data.Unacknowledged + '</td></tr>');
              $table.append('<tr><td>FixReady</td><td>' + result.data.FixReady + '</td></tr>');
              $table.append('<tr><td>Ignored</td><td>' + result.data.Ignored + '</td></tr>');
              $table.append('<tr><td>Solved</td><td>' + result.data.Solved + '</td></tr>');
              $table.append('<tr><td>Applications</td><td>' + result.data.Applications + '</td></tr>');
              $table.append('<tr><td>Users</td><td>' + result.data.Users + '</td></tr>');
              $table.append('<tr><td>Groups</td><td>' + result.data.Groups + '</td></tr>');
              return $modal.modal();
            } else {
              return alert(result.message);
            }
          },
          error: function() {
            return alert("error");
          },
          dataType: "json"
        });
        return true;
      });
    }
  });

}).call(this);
