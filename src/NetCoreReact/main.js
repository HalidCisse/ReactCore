const { app, BrowserWindow, autoUpdater, dialog, screen: electronScreen } = require('electron')
const debug             = require('electron-debug')
const os                = require('os')
const path              = require('path')
const unhandled         = require('electron-unhandled')
const Store             = require('electron-store')
const logger            = require('electron-timber')
const { is, darkMode }  = require('electron-util')
const windowStateKeeper = require('electron-window-state')

const { default: installExtension, REACT_DEVELOPER_TOOLS } = require('electron-devtools-installer')

const store          = new Store()

debug()
unhandled()

try {
    require('electron-reloader')(__dirname, { 
        electron: path.join(__dirname, 'node_modules/.bin/electron.cmd')
    })
} catch (_) { }

let mainWindow
let serverProcess

const createWindow = async () =>  {
    process.env.REACT_APP_DESKTOP = false

    let lastWindowState = windowStateKeeper({
        defaultWidth: 800,
        defaultHeight: 600
    })

    const isDarkMode = store.get('darkMode')

    mainWindow = new BrowserWindow({
        title:'NetCore React',
        show: false,
        backgroundColor:'white',
        titleBarStyle:'hidden',

        x: lastWindowState.x,
        y: lastWindowState.y,
        width: lastWindowState.width,
        height: lastWindowState.height,
        minWidth: 400,
        minHeight: 200,

        darkTheme: isDarkMode,
        webPreferences: {
            sandbox: false,
            nodeIntegration: is.development,
            devTools:true,

            //preload: path.join(__dirname, 'preload.js'),
            nativeWindowOpen: true,
            contextIsolation: true,
            spellcheck: true,
            plugins: true
        }
    })

    let previousDarkMode = darkMode.isEnabled;
    darkMode.onChange(() => {
        if (darkMode.isEnabled !== previousDarkMode) {
            previousDarkMode = darkMode.isEnabled;
            win.webContents.send('set-dark-mode');
        }
    })

    lastWindowState.manage(mainWindow)
    mainWindow.once('ready-to-show', () => mainWindow.show())
    mainWindow.loadURL('http://localhost:5000')
    mainWindow.on('closed', () => mainWindow = null)

    mainWindow.webContents.on('dom-ready', async () =>{
        if (store.get('launchMinimized') || app.getLoginItemSettings().wasOpenedAsHidden) {
            mainWindow.hide();
            tray.create(mainWindow);
        } else {
            mainWindow.show();
        }
    })

    if (is.development){
        logger.log(`isDev ${is.development}`)
        mainWindow.webContents.once('dom-ready', () => {
            mainWindow.webContents.openDevTools()
        })

        installExtension(REACT_DEVELOPER_TOOLS)
            .then((name) => logger.log(`Added Extension:  ${name}`))
            .catch((err) => logger.log('An error occurred: ', err));
    }
}

const runServer = async () => {
    logger.log('Starting server ...');

    store.set('server:url', '🦄')
    logger.log(store.get('server:url'))
    
    if (is.development) {
        if (mainWindow == null) await createWindow()
    } else {
        let proc = require('child_process').spawn;
        let serverPath = path.join(__dirname, os.platform() === 'darwin' ? '..//server//NetCoreReact' : '..\\server\\NetCoreReact.exe')

        serverProcess = proc(serverPath)
        serverProcess.stdout.on('data', async (data) => {
            logger.log(`stdout: ${data}`)
            if (mainWindow == null) await createWindow()
        })
    }
}

app.on('ready', ()=> {
    electronScreen.on('display-removed', () => {
        const [x, y] = mainWindow.getPosition()
        mainWindow.setPosition(x, y)
    })

    runServer()
})

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') app.quit()
})

app.on('activate', async () => {
    if (mainWindow === null) {
        await createWindow()
    }
})

if (!app.requestSingleInstanceLock()) {
    app.quit()
}

app.on('second-instance', () => {
    if (mainWindow) {
        if (mainWindow.isMinimized()) {
            mainWindow.restore();
        }

        mainWindow.show();
    }
})

process.on('exit', ()=> {
    logger.log('exit')
    if(serverProcess)
        serverProcess.kill()
})

if (!is.development) {
    (async () => {
        autoUpdater.logger = logger

        const FOUR_HOURS = 1000 * 60 * 60 * 4
        setInterval(async () => {
            await autoUpdater.checkForUpdates()
        }, FOUR_HOURS);

        await autoUpdater.checkForUpdates()
    })();
}

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
    logger.error('There was a problem updating the application')
    logger.error(message)
})

process.env['ELECTRON_DISABLE_SECURITY_WARNINGS'] = true
app.allowRendererProcessReuse = false;