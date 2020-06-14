async function send(url, body) {
    let result = await fetch(url, {
        method: body? 'POST': 'GET',
        headers: { 'Content-Type': 'application/json' },
        body: body? JSON.stringify(body): null
    })
    .then(response => {
        if (response.status < 200 || response.status > 299) {
            throw Error(response.status || 'Call failed');
        }
        return response.json();
    })
    .then(json => ({ data: json, isError: false }))
    .catch(error => ({ data: error.message, isError: true }));

    return result || { data: 'Unknown error', isError: true };
}


export default class Comms {
    constructor(isDebug, isTrace) {
        this._isTrace = !!isTrace;
        this._logger = isDebug? (message) => console.log(message): () => { };
        this._socket = null;

        this.on = null;
        this.received = null;
    }


    async login(username, password) {
        const url = `wss://${window.location.hostname}:${window.location.port}/client`;

        try {
            if (this._socket) {
                this.logout();
            }

            const response = await send('login', {
                Username: username,
                Password: password
            });

            if (response.isError || !response.data) {
                this._logger(`COMMS>> Login failure: ${response.data}`);
                return false;
            } else {
                this._logger(`COMMS>> Login success yielded token: ${JSON.stringify(response.data)}`);
                this._logger(`COMMS>> Opening channel.`);

                const socket = new WebSocket(url);
                const self = this;
                
                this._socket = socket;
                this._token = response.data.loginToken;

                socket.onclose = function(event) {
                    if (socket === self._socket) {
                        self.logout('Close by server.');
                        
                        if (self.on) {
                            self.on('close', event);
                        }
                    } else {
                        self._logger(`COMMS>> Close for superseded socket: closing it.`);
                        socket.close();
                    }
                };

                socket.onerror = function(error) {
                    if (socket === self._socket) {
                        if (self.on) {
                            self.logout(error? error.message: 'Unknown error.');
                            self.on('error', error);
                        }
                    } else {
                        self._logger(`COMMS>> Error for superseded socket: closing it.`);
                        socket.close();
                    }
                };

                socket.onmessage = (event) => {
                    try {
                        if (socket === self._socket) {
                            if (self._isTrace) {
                                console.log(`COMMS>> received message: ${event.data}`);
                            }
                            if (self.received) {
                                self.received(event.data);
                            }
                        } else {
                            self._logger(`COMMS>> Message for superseded socket: closing it.`);
                            socket.close();
                        }
                    } catch (error) {
                        console.log(`COMMS!! error in onmessage: ${error.message}`);
                    }
                };

                socket.onopen = () => {
                    try {
                        if (socket === self._socket) {
                            self._logger(`COMMS>> Channel open: sending token: ${self._token}`);
                            socket.send(self._token);
                        } else {
                            self._logger(`COMMS>> Channel open for superseded socket: closing it.`);
                            socket.close();
                        }
                    } catch (error) {
                        console.log(`COMMS!! error in onopen: ${error.message}`);
                    }
                };

                return true;
            }
        } catch (error) {
            console.log(`COMMS!! Exception during login: ${error.message}`);
            false;
        }
    }


    logout(message) {
        if (this._socket) {
            this._logger(`COMMS>> Logout: ${message}`);
            this._socket.onclose = null;
            this._socket.onerror = null;
            this._socket.onmessage = null;
            this._socket.onopen = null;
            this._socket.close();
            this._socket = null;
        }
    }


    send(message) {
        if (this._socket) {
            if (this._isTrace) {
                this._logger(`COMMS>> Sending message: ${message}`);
            }
            this._socket.send(message);
        } else {
            this._logger(`COMMS>> Sending message when already closed.`);
        }
    }
}