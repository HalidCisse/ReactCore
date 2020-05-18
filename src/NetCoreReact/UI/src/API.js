/* eslint-disable no-undef */
import { HubConnectionBuilder } from '@microsoft/signalr'
import { http } from './http'

let hub = null
export default class API {
    
    static ws = {
        open: () => {
            if ((!hub || (hub && hub.stopped)) && localStorage.Authorization) {
                hub = new HubConnectionBuilder()
                    .withUrl(API + 'rm?authorization=' + localStorage.Authorization)
                    .build()
                hub
                    .start()
                    .then(() => {
                        delete hub.stopped
                        console.log('RTM subscribed')
                    })
                    .catch(e => {
                        hub = null
                        console.error(e)
                    })
                hub.onclose(async () => {
                    if (!hub) return
                    hub.stopped = true
                    if (hub.stopping) console.log('RTM Stopped')
                    else {
                        hub = null
                        console.error('RTM Failed')
                    }
                })
            }
            return hub
        },

        on: func => {
            if (eSchool.ws.open()) hub
                .on('rm', func)
        },

        stop: () => {
            if (eSchool.ws.open()) {
                hub.stopping = true
                hub.stop()
                hub = null
                console.log('RTM stopped')
            }
        }
    }
    
    static files = {
        drives: () =>
            http.get('/files'),
        
        get: (path, skip, limit) =>
            http.get('/files/content?limit=' + (limit || '100') + '&skip=' + (skip || '0') + '&path=' + (path || ''))
    }
}
