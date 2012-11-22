jQuery -> 
	$body = $('div#alerts');
	$alerts = null;
	alertHeight = 58
	alertHeaderHeight = 46

	if $body.length > 0

		$("div#head-links").delegate 'a#show-alerts', 'click', (e) ->
			e.preventDefault()
			if $body.find('div.alert').length == 0
				$alerts = new Alerts()
				$alerts.show(() -> 
					if $body.find('div.alert').length == 0
						$alerts.setTimeout()
						alert "You have no alerts at present"
				)
			else
				$body.show()
				$body.animate({
					height: ($body.find('div.alert').length * alertHeight) + alertHeaderHeight
				}, 1000)

		$(document).ready () -> 
			$alerts = new Alerts()
			if ($.cookie("alerts") == null || $.cookie("alerts") == "show") && $alerts.hasTimedOut()
				$alerts.show()
				$alerts.setTimeout()

		$body.delegate 'a#hidealerts', 'click', (e) -> 
			$alerts.hide()
			false	

		$body.delegate 'a#dismissalerts', 'click', (e) -> 
			Errordite.Spinner.disable()
			$.post '/alerts/dismissall'
			Errordite.Spinner.enable()
			$alerts.setTimeout()
			false	

		class Alerts
			bind: (alerts) ->

				$('div').remove('.alert')

				for a in alerts.data
					$body.append '<div class="alert alert-success" data-alert-id="' + a.Id + '" data-alert-utc="' + a.Date + '"><a class="close" data-dismiss="alert">X</a><h4 class="alert-heading">' + a.Header + ' on ' + a.Date + '</h4>' + a.Message + '</div>'

				$body.css {
					left: ($(window).width()/2) - 300,
					display: 'block'
				}

				$body.animate({
					height: (alerts.data.length * alertHeight) + alertHeaderHeight
				}, 500)

				$('.alert').bind 'closed', () ->
					$this = $(this)

					alertCount = $body.find('div.alert').length
					
					if alertCount <= 3
						if alertCount == 1
							$body.animate({	height: 0}, 500, () -> $body.hide())
						else
							$body.animate({	height: ((alertCount - 1) * alertHeight) + alertHeaderHeight - 5}, 500)

					Errordite.Spinner.disable()
					$.post '/alerts/dismiss',
						id: $this.closest('[data-alert-id]').data 'alert-id'
					Errordite.Spinner.enable()

			show: (complete) -> 
				this.setCookie("show");
				Errordite.Spinner.disable()
				$.get '/alerts/get',
					(alerts) -> 
						if alerts.data.length > 0
							$alerts.bind(alerts)
						complete() if complete?
				Errordite.Spinner.enable()
							
			
			hide: () -> 
				this.setCookie("hide");
				$body.animate {	height: 0 }, 500, () -> $body.hide();

			setCookie: (val) -> 
				expiry = new Date(); 
				expiry.setMinutes expiry.getMinutes() + 60;
				$.cookie "alerts", val, { expires: expiry }
				
			setTimeout: () -> 
				expiry = new Date(); 
				expiry.setMinutes expiry.getMinutes() + 5;
				$.cookie "alerts-timeout", "", { expires: expiry }

			hasTimedOut: () ->
				return $.cookie("alerts-timeout") == null

								