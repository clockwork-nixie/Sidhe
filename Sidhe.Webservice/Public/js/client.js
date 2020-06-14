import Comms from './comms.js';
import Ui from './ui.js';

const comms = new Comms(true, false);
const ui = new Ui(window.document, comms);

comms.on = (eventName, data) => ui.routeEvent(eventName, data);
comms.received = (message) => ui.routeMessage(message);
