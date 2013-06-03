(function() {

  jQuery(function() {
    var $root, Monitoring, monitoring;
    $root = $("section#monitoring");
    if ($root.length > 0) {
      $root.delegate("ul.dropdown-menu li a", "click", function(e) {
        var $this;
        e.preventDefault();
        $this = $(this);
        return Errordite.Confirm.show($this.data('check'), {
          okCallBack: function() {
            return monitoring.performAction($this.data('action'));
          }
        });
      });
      $('th :checkbox').on('click', function() {
        return $(this).closest('table').find('td :checkbox').prop('checked', $(this).is(':checked'));
      });
      Monitoring = (function() {

        Monitoring.name = 'Monitoring';

        function Monitoring() {}

        Monitoring.prototype.performAction = function(action) {
          var $form;
          $root.find('input#OrgId').val($root.find('input#OrganisationId').val());
          $root.find('input#Svc').val($root.find('select#Service').val());
          $form = $root.find('form#actionForm');
          $form.attr('action', $form.attr('action').replace('delete', action));
          $form.submit();
          return true;
        };

        return Monitoring;

      })();
      monitoring = new Monitoring();
      return true;
    }
  });

}).call(this);
