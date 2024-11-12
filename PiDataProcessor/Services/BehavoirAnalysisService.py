from EventSystem import event_system

class CurrentBehaviorModule:
    def __init__(self):
        event_system.subscribe(self.process_data, 'message_received')

    def process_data(self, data):
        pass