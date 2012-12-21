(function() {

  jQuery(function() {
    var $activeModal, $root, maybeEnableBatchStatus;
    $root = $('section#issues');
    $activeModal = null;
    if ($root.length > 0) {
      $root.delegate('form#actionForm', 'submit', function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        return $.post($this.attr('action'), $this.serialize(), function(data) {
          return window.location.reload();
        }).error(function(e) {
          if ($activeModal != null) {
            $activeModal.find('div.error').removeClass('hidden');
            return $activeModal.find("div.error span").text("An error occurred, please close the modal window and try again.");
          }
        });
      });
      $root.delegate('ul.dropdown-menu li input', 'click', function(e) {
        return e.stopPropagation();
      });
      $root.delegate('ul.dropdown-menu li a', 'click', function(e) {
        e.preventDefault();
        return $(this).closest('ul').find('li :checkbox').prop('checked', true);
      });
      $root.delegate('ul.dropdown-menu li', 'click', function(e) {
        var $chk, $this;
        $this = $(this);
        $chk = $this.closest('li').children('input');
        $chk.attr('checked', !$chk.attr('checked'));
        return false;
      });
      $root.delegate('ul#action-list ul.dropdown-menu li a', 'click', function() {
        var $modal, $this;
        $this = $(this);
        $modal = $root.find('div#' + $this.attr('data-val-modal'));
        if ($modal === null) {
          return null;
        }
        $root.find('input[type="hidden"]#Action').val($modal.attr("id"));
        $modal.find('.batch-issue-count').text($(':checkbox:checked[name=issueIds]').length);
        $modal.find('.batch-issue-plural').toggle($(':checkbox:checked[name=issueIds]').length > 1);
        if ($modal.find('.batch-issue-status').length > 0) {
          $modal.find('.batch-issue-status').text($this.attr('data-val-status'));
          $root.find('input[type="hidden"]#Status').val($this.attr('data-val-status').replace(' ', ''));
        }
        $activeModal = $modal;
        return $modal.modal();
      });
      $('th :checkbox').on('click', function() {
        $(this).closest('table').find('td :checkbox').prop('checked', $(this).is(':checked'));
        return maybeEnableBatchStatus();
      });
      maybeEnableBatchStatus = function() {
        return $('ul#action-list').toggle(!!$(':checkbox:checked[name=issueIds]').length);
      };
      $root.delegate(':checkbox[name=issueIds]', 'click', function() {
        return maybeEnableBatchStatus();
      });
      return maybeEnableBatchStatus();
    }
  });

}).call(this);
