(function() {

  jQuery(function() {
    var $activeModal, $body, init, maybeEnableBatchStatus;
    $body = $('div#issues');
    $activeModal = null;
    if ($body.length > 0) {
      init = new Initalisation();
      init.datepicker($body);
      $body.delegate('form#actionForm', 'submit', function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        return $.post($this.attr('action'), $this.serialize(), function(data) {
          return window.location.reload();
        }).error(function(e) {
          if ($activeModal != null) {
            $activeModal.find('div.alert').removeClass('hidden');
            return $activeModal.find("div.alert h4").text("An error occurred, please close the modal window and try again.");
          }
        });
      });
      $body.delegate('div.dropdown-small ul.dropdown-menu li input', 'click', function(e) {
        return e.stopPropagation();
      });
      $body.delegate('div.dropdown-small ul.dropdown-menu li a', 'click', function(e) {
        e.preventDefault();
        return $(this).closest('ul').find('li :checkbox').prop('checked', true);
      });
      $body.delegate('div.dropdown-small ul.dropdown-menu li', 'click', function(e) {
        var $chk, $this;
        $this = $(this);
        $chk = $this.closest('li').children('input');
        $chk.attr('checked', !$chk.attr('checked'));
        return false;
      });
      $body.delegate('div.action ul.dropdown-menu li a', 'click', function() {
        var $modal, $this;
        $this = $(this);
        $modal = $body.find('div#' + $this.attr('data-val-modal'));
        if ($modal === null) {
          return null;
        }
        $body.find('input[type="hidden"]#Action').val($modal.attr("id"));
        $modal.find('.batch-issue-count').text($(':checkbox:checked[name=issueIds]').length);
        $modal.find('.batch-issue-plural').toggle($(':checkbox:checked[name=issueIds]').length > 1);
        if ($modal.find('.batch-issue-status').length > 0) {
          $modal.find('.batch-issue-status').text($this.attr('data-val-status'));
          $body.find('input[type="hidden"]#Status').val($this.attr('data-val-status').replace(' ', ''));
        }
        $activeModal = $modal;
        return $modal.modal();
      });
      $('th :checkbox').on('click', function() {
        $(this).closest('table').find('td :checkbox').prop('checked', $(this).is(':checked'));
        return maybeEnableBatchStatus();
      });
      maybeEnableBatchStatus = function() {
        return $('div.action').toggle(!!$(':checkbox:checked[name=issueIds]').length);
      };
      $body.delegate(':checkbox[name=issueIds]', 'click', function() {
        return maybeEnableBatchStatus();
      });
      return maybeEnableBatchStatus();
    }
  });

}).call(this);
