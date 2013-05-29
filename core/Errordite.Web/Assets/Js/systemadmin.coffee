jQuery -> 
	$orgroot = $('section#organisations');
	$cacheroot = $('section#caching');

	if $cacheroot.length > 0
		$cacheroot.delegate "select#CacheEngine", "change", ->
			$this = $(this)
			index = window.location.href.indexOf("?")

			if index is -1
				window.location = window.location.href + "?engine=" + $this.val()
			else
				window.location = window.location.href.substring(0, index) + "?engine=" + $this.val()


	if $orgroot.length > 0

		$orgroot.delegate 'form#suspendForm', 'submit', (e) -> 
			e.preventDefault()
			$this = $ this

			$.post $this.attr('action'), $this.serialize(), (data) -> 
				window.location.reload()

		$orgroot.delegate 'a.suspend', 'click', (e) -> 
			e.preventDefault()
			$this = $ this
			$modal = $orgroot.find('div#suspend-modal')
			return null if $modal == null
			$modal.find('input[type=hidden]').val $this.data 'val'
			$modal.modal()

		$orgroot.delegate 'input[type=submit].activate', 'click', (e) -> 
			$this = $ this

			if confirm "are you sure you want to activate this organisation?"
				return true

			e.preventDefault()
			false

		$orgroot.delegate 'input[type=submit].delete', 'click', (e) -> 
			$this = $ this

			if confirm "are you sure you want to delete this organisation, all data will be permenantly deleted?"
				return true

			e.preventDefault()
			false

		$orgroot.delegate 'a.stats', 'click', (e) -> 
			e.preventDefault()
			$this = $ this
			$modal = $orgroot.find('div#stats-modal')
			return null if $modal == null

			$.ajax
				url: "/system/organisations/stats?organisationId=" +  $this.data('orgid')
				success: (result) ->
					if result.success
						$table = $modal.find('table#stats');
						$table.empty();
						$table.append('<tr><td>Issues</td><td>' + result.data.Issues + '</td></tr>')
						$table.append('<tr><td>Acknowledged</td><td>' + result.data.Acknowledged + '</td></tr>')
						$table.append('<tr><td>Unacknowledged</td><td>' + result.data.Unacknowledged + '</td></tr>')
						$table.append('<tr><td>FixReady</td><td>' + result.data.FixReady + '</td></tr>')
						$table.append('<tr><td>Ignored</td><td>' + result.data.Ignored + '</td></tr>')
						$table.append('<tr><td>Solved</td><td>' + result.data.Solved + '</td></tr>')
						$table.append('<tr><td>Applications</td><td>' + result.data.Applications + '</td></tr>')
						$table.append('<tr><td>Users</td><td>' + result.data.Users + '</td></tr>')
						$table.append('<tr><td>Groups</td><td>' + result.data.Groups + '</td></tr>')
						$modal.modal()
					else
						alert result.message
				error: ->
					alert "error"
				dataType: "json"

			true
			