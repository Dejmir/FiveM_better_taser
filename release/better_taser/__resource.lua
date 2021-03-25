-- specify the root page, relative to the resource
ui_page 'ui/index.html'

-- every client-side file still needs to be added to the resource packfile!
files {
    'ui/index.html',
	'ui/style.css',
	'ui/listener.js',
	
	'ui/batteries.png',
	'ui/taser.png',
	'ui/close.png',
	
	
	'Newtonsoft.Json.dll'
}


client_scripts {'fivem_taser.net.dll', 'client.lua'}
server_scripts {'fivem_taser_server.net.dll'}