/* eslint-disable no-undef */
import {HttpTransportType, HubConnectionBuilder, LogLevel} from '@aspnet/signalr'
import { http } from './http'

let hub = null
const delay = ms => new Promise(_ => setTimeout(_, ms))

export default class API {
    
    static ws = {
        open: () => {
            if ((!hub || (hub && hub.stopped)) && localStorage.Authorization) {
                hub = new HubConnectionBuilder()
                    .configureLogging(LogLevel.Error)
                    .withUrl(`ws?authorization=${localStorage.Authorization}`, {transport:HttpTransportType.WebSockets, skipNegotiation:true})
                    .build()
                hub.start()
                    .then(() => delete hub.stopped)
                    .catch(e => {
                        hub = null
                        console.error(e)
                    })
                hub.onclose(async () => {
                    if (!hub) return
                    hub.stopped = true
                    if (hub.stopping) {
                        console.log('WS Stopped')
                    } else {
                        hub = null
                        console.error('WS Failed')
                    }
                })
            }
            return hub
        },

        on: func => {
            if (API.ws.open()) hub.on('ws', func)
        },

        stop: () => {
            if (API.ws.open()) {
                hub.stopping = true
                hub.stop()
                hub = null
                console.log('WS stopped')
            }
        },
    }
    
    static files = {
        drives: () =>
            http.get('/files'),
        
        get: (path, skip, limit) =>
            http.get('/files/content?limit=' + (limit || '100') + '&skip=' + (skip || '0') + '&path=' + (path || ''))
    }
}
