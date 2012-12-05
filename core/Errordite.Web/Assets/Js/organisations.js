(function() {

  jQuery(function() {
    var $body;
    $body = $('div#organisations');
    if ($body.length > 0) {
      $body.delegate('form#suspendForm', 'submit', function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        return $.post($this.attr('action'), $this.serialize(), function(data) {
          return window.location.reload();
        });
      });
      $body.delegate('a.suspend', 'click', function(e) {
        var $modal, $this;
        e.preventDefault();
        $this = $(this);
        $modal = $body.find('div#suspend-modal');
        if ($modal === null) {
          return null;
        }
        $modal.find('input[type=hidden]').val($this.data('val'));
        return $modal.modal();
      });
      $body.delegate('input[type=submit].activate', 'click', function(e) {
        var $this;
        $this = $(this);
        if (confirm("are you sure you want to activate this organisation?")) {
          return true;
        }
        e.preventDefault();
        return false;
      });
      return $body.delegate('input[type=submit].delete', 'click', function(e) {
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
