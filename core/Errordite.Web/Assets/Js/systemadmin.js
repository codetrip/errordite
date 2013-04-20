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
      return $orgroot.delegate('input[type=submit].delete', 'click', function(e) {
        var $this;
        $this = $(this);
        if (confirm("are you sure you want to delete this organisation, all data will be permenantly deleted?")) {
          return true;
        }
        e.preventDefault();
        return false;
      });
    }
  });

}).call(this);
