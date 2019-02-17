const electron = require('electron')
const debug = require('electron-debug')

debug()
const app            = electron.app
const isDev          = require('electron-is-dev');
const os             = require('os')
const path           = require('path')
const url            = require('url')

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let mainWindow
let serverProcess

function createWindow () {
    // Create the browser window.
    mainWindow = new electron.BrowserWindow({
        width: 800, 
        height: 600,
        webPreferences: {
            nodeIntegration: true,
            devTools:true
            //devTools: isDev
        }
    })
    
    if(isDev){
        mainWindow.loadURL('http://localhost:5000')
    } else {
        mainWindow.loadURL('http://localhost:5000')
        // and load the index.html of the app.
        // mainWindow.loadURL(url.format({
        //     pathname: path.join(__dirname, 'index.html'),
        //     protocol: 'file:',
        //     slashes: true
        // }))
    }

    // Open the DevTools.
    mainWindow.webContents.openDevTools()

    // Emitted when the window is closed.
    mainWindow.on('closed', () => {
        mainWindow = null
    })
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', startApi)

// Quit when all windows are closed.
app.on('window-all-closed', () => {
    // On OS X it is common for applications and their menu bar
    // to stay active until the user quits explicitly with Cmd + Q
    if (process.platform !== 'darwin') {
        app.quit()
    }
})

app.on('activate', () => {
    // On OS X it's common to re-create a window in the app when the
    // dock icon is clicked and there are no other windows open.
    if (mainWindow === null) {
        createWindow()
    }
})

function startApi() {

    console.log('isDevelopement '+ isDev) 
    if(isDev){
        if (mainWindow == null) {
            createWindow()
        }
    } else {
        let proc = require('child_process').spawn;
        //  run server
        let serverPath = path.join(__dirname, '..\\server\\Acembly.Ftx.exe')
        if (os.platform() === 'darwin') {
            serverPath = path.join(__dirname, '..//server//Acembly.Ftx')
        }
        
        console.log('serverPath ' + serverPath)
        
        serverProcess = proc(serverPath)
        serverProcess.stdout.on('data', (data) => {
            writeLog(`stdout: ${data}`);
            if (mainWindow == null) {
                createWindow();
            }
        })
    }
}

//Kill process when electron exits
process.on('exit', function () {
    writeLog('exit')
    if(serverProcess)
        serverProcess.kill()
})

function writeLog(msg){
    console.log(msg)
}