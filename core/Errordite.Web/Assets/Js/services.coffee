(($) ->
  $root = $("div#system-status")
  Dashboard = undefined
  dashboard = undefined
  currentEndpoint = undefined
  currentQueue = undefined
  timeout = undefined

  if $root.length > 0

    $root.delegate "a.purge", "click", (e) ->
      e.preventDefault()
      $this = $(this)
      return false  unless confirm("Are you sure you want to delete the selected messages?")
      dashboard.deleteMessages $this.data("queue"), null, $this.data("servicename")

    $root.delegate "a.retry", "click", (e) ->
      e.preventDefault()
      $this = $(this)
      return false  unless confirm("Are you sure you want to retry the selected messages?")
      dashboard.returnToSource $this.data("queue"), null, $this.data("servicename")

	$root.delegate "select#RavenInstanceId", "change", (e) ->
      e.preventDefault()
      $this = $(this)
      dashboard.switchInstance $this.val()

    Dashboard = (->
      Dashboard = ->
        @container = $("div.service-info-container")

      Dashboard::bindEvents = ->
        $("th :checkbox").on "click", ->
          $(this).closest("table").find("td :checkbox").prop "checked", $(this).is(":checked")

      Dashboard::poll = ->
        $.ajax
          url: "/systemadmin/styles/admin/updatesystemstatus"
          success: (result) ->
            console.log "success"
            return dashboard.bind(result.Data)  if result.Success
            false

          error: ->
            false

          dataType: "json"

        true

      Dashboard::deleteMessages = (queueName, serviceName) ->
        $.ajax
          url: "/system/services/deletemessages"
          data:
            instanceId: $('select#RavenInstanceId').val()
            queueName: queueName
            serviceName: serviceName

          success: (result) ->
            if result.Success
                return dashboard.poll()
            else
              alert "Failed to delete messages, please try again."
            true

          error: ->
            alert "Failed to delete messages, please try again."

          dataType: "json"
          type: "POST"

        true

      Dashboard::returnToSource = (queueName, serviceName) ->
        dashboard.pollingEnabled = false
        $.ajax
          url: "/system/services/returntosource"
          data:
            instanceId: $('select#RavenInstanceId').val()
            queueName: queueName
            serviceName: serviceName

          success: (result) ->
            if result.Success
                return dashboard.poll()
            else
              alert "Failed to return error messages to their source queue, please try again."
            true

          error: ->
            dashboard.pollingEnabled = true
            alert "Failed to return error messages to their source queue, please try again."

          dataType: "json"
          type: "POST"

        true

      Dashboard::swapInstance = (instanceId) ->
        window.location = "/system/services?ravenInstanceId=" + instanceId
        true
		
      Dashboard::reload = () ->
        window.location = "/system/services?ravenInstanceId=" + $('select#RavenInstanceId').val()
        true

      Dashboard::bind = (data) ->
        dashboard.container.hide "drop",
          complete: ->
            dashboard.container.html data
            dashboard.container.show "slow"

        true

      Dashboard
    )()
    dashboard = new Dashboard()
    timeout = setTimeout(dashboard.poll, 15000)
) jQuery