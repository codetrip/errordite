(function() {
  jQuery(function() {
    var $orgroot;

    $orgroot = $('section#organisations');
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
      $orgroot.delegate('a.activate', 'click', function(e) {
        var $form, $this;

        $this = $(this);
        if (confirm("Are you sure you want to activate this organisation?")) {
          $form = $this.closest('form');
          $form.submit();
        }
        e.preventDefault();
        return false;
      });
      return $orgroot.delegate('a.delete', 'click', function(e) {
        var $form, $this;

        $this = $(this);
        if (confirm("Are you sure you want to delete this organisation, all data will be permenantly deleted?")) {
          $form = $this.closest('form');
          $form.submit();
        }
        e.preventDefault();
        return false;
      });
    }
  });

}).call(this);
