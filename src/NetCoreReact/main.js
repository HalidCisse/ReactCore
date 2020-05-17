const {app, BrowserWindow, autoUpdater, dialog} = require('electron')
const debug          = require('electron-debug')
const isDev          = require('electron-is-dev');
const os             = require('os')
const path           = require('path')
const debugMenu      = require('debug-menu')

debug()

let mainWindow
let serverProcess

const createWindow = async () =>  {
    process.env.REACT_APP_DESKTOP = true
    mainWindow = new BrowserWindow({
        title:'NetCore React',
        backgroundColor:'white',
        titleBarStyle:'hidden',
        show: false,
        width: 800,
        height: 600,
        webPreferences: {
            nodeIntegration: false,
            devTools:true
        }
    })

    mainWindow.once('ready-to-show', () => mainWindow.show())
    mainWindow.loadURL('http://localhost:5000')
    mainWindow.on('closed', () => mainWindow = null)

    if (isDev){
        mainWindow.webContents.openDevTools()
        const menu = Menu.buildFromTemplate([{
            label: 'Debug',
            submenu: debugMenu.windowDebugMenu(win)
        }]);

        if (process.platform !== 'darwin') {
            win.setMenu(menu);
        } else {
            electron.Menu.setApplicationMenu(menu);
        }
    }
}

const runServer = async () => {
    if (isDev) {
        if (mainWindow == null) await createWindow()
    } else {
        let proc = require('child_process').spawn;
        let serverPath = path.join(__dirname, os.platform() === 'darwin' ? '..//server//NetCoreReact' : '..\\server\\NetCoreReact.exe')

        serverProcess = proc(serverPath)
        serverProcess.stdout.on('data', async (data) => {
            writeLog(`stdout: ${data}`)
            if (mainWindow == null) await createWindow()
        })
    }
}

app.on('ready', runServer)

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') app.quit()
})

app.on('activate', async () => {
    if (mainWindow === null) {
        await createWindow()
    }
})

process.on('exit', ()=> {
    writeLog('exit')
    if(serverProcess)
        serverProcess.kill()
})

autoUpdater.on('update-downloaded', (event, releaseNotes, releaseName) => {
    const dialogOpts = {
        type: 'info',
        buttons: ['Restart', 'Later'],
        title: 'Application Update',
        message: process.platform === 'win32' ? releaseNotes : releaseName,
        detail: 'A new version has been downloaded. Restart the application to apply the updates.'
    }
    dialog.showMessageBox(dialogOpts, (response) => {
        if (response === 0) autoUpdater.quitAndInstall()
    })
})

autoUpdater.on('error', message => {
    console.error('There was a problem updating the application')
    console.error(message)
})

writeLog = msg => console.log(msg)