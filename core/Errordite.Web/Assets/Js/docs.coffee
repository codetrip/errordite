
jQuery -> 
	$body = $ 'section#clients'

	if $body.length > 0
		$body.delegate '[data-toggle="tab"]', 'click', (e) -> 
            e.preventDefault()
            $this = $ this
            $container = $this.closest('.sidenav')
            $container.find('div.sidenav-tab').removeClass('active')
            $container.find('li.active').removeClass('active')
            $container.find('div.sidenav-tab#' + $this.data('tab')).addClass('active')
            $this.closest('li').addClass('active')
            false
	