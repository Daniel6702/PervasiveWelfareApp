class EventSystem:
    '''
    Singleton EventSystem class.
    Maintains a subscription model where various functions (subscribers) can subscribe to specific event types.
    When an event of a subscribed type occurs, the EventSystem runs all subscribed functions with the published data.
    '''
    _instance = None

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(EventSystem, cls).__new__(cls)
            cls._instance.__initialized = False
        return cls._instance

    def __init__(self):
        if self.__initialized:
            return
        self.subscribers = dict()
        self.__initialized = True

    def subscribe(self, fn, event_type):
        '''Allows a function 'fn' to subscribe to a specific event type 'event_type'.'''
        if event_type not in self.subscribers:
            self.subscribers[event_type] = []
        self.subscribers[event_type].append(fn)

    def publish(self, data, event_type):
        '''Publishes event data to all functions subscribed to the 'event_type'.'''
        if event_type in self.subscribers:
            for fn in self.subscribers[event_type]:
                fn(data)

event_system = EventSystem()