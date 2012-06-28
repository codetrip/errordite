jQuery -> 
	$issue = $('div#issue');

	if $issue.length > 0

		$(document).ready () -> 
			loadTabData($ 'ul#issue-tabs li.active a.tablink')

		$issue.delegate 'input[type="button"].confirm', 'click', () ->
			$this = $ this
			if confirm "Are you sure you want to delete all errors associated with this issue?" 
				$.post '/issue/purge', 'issueId=' + $this.attr('data-val'), (data) -> 
					renderErrors '/issue/errors?issueId=' + $this.attr('data-val')
					$('span#instance-count').text "0"

		loadTabData = ($tab) ->
			if not $tab.data 'loaded'
				if $tab.data("val") == "reports"				
					renderDistribution()
				else if $tab.data("val") == "errors"
					renderErrors '/issue/errors?' + $('form#errorsForm').serialize()
				$tab.data 'loaded', true	

		renderDistribution = () -> 
			$.get "/issue/getreportdata?issueId=" + $issue.find('input[type="hidden"]#IssueId').val(),
				(data) -> 
					d = $.parseJSON(data.data)

					$.jqplot('distribution', d.series, {
						seriesDefaults: {
							renderer:$.jqplot.BarRenderer
						},
						axes: {
							xaxis: {
							renderer: $.jqplot.CategoryAxisRenderer,
							ticks: d.ticks
							}
						}
					});

		renderErrors = (url) -> 
			$node = $issue.find('div#error-criteria')
			$.get url,
				(data) -> 
					$node.html(data)
					init = new Initalisation()
					#init.init(true,	(uri) -> renderErrors uri);
					init.datepicker($issue);
					$('div.content').animate({scrollTop : 0},'slow')

		$issue.delegate 'form#errorsForm', 'submit', (e) ->
			e.preventDefault()
			$this = $ this
			renderErrors '/issue/errors?' + $this.serialize()

		$issue.delegate 'select#Status', 'change', () -> 
			$this = $ this

			if $this.val() == 'Ignorable'
				$issue.find('li.checkbox').removeClass('hidden');
			else
				$issue.find('li.checkbox').addClass('hidden');

			false	

		if $issue.find('select#Status').val() == 'Ignorable'
			$issue.find('li.checkbox').removeClass('hidden')

		$('#issue-tabs .tablink').bind 'shown', (e) -> 
			loadTabData $ e.currentTarget		

