$(function() {
    /* open the UI for user */
    window.addEventListener('message', function(event) {
        if(event.data.type == "open"){
            $('#main').css('display', 'flex');
            
        } else if (event.data.type == "close"){
            $('#main').css('display', 'none');
        }
        else if (event.data.type == "cartridgelow"){
            $('#spancartridge').css('color', 'red');
        }
        else if (event.data.type == "batterieslow"){
            $('#spanbatteries').css('color', 'red');
        }
        else if (event.data.type == "cartridgeloaded"){
            $('#spancartridge').css('color', 'black');
        }
        else if (event.data.type == "batteriesloaded"){
            $('#spanbatteries').css('color', 'black');
        }
    });
    /*window.addEventListener("click", () => {
        SendData("quit");
    })*/
    document.getElementById("cartridge").addEventListener("click", () => {
        SendData("cartridge");
    });
    document.getElementById("batteries").addEventListener("click", () => {
        SendData("batteries");
    });
    document.getElementById("destroy").addEventListener("click", () =>{
        SendData("destroy");
    });

    function SendData(data, closeui = true){
        if(closeui == true) $('#main').css('display', 'none');
        fetch(`https://${GetParentResourceName()}/maincallback`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json; charset=UTF-8',
            },
            body: JSON.stringify({
                callback: data
            })
        });
    }
});	