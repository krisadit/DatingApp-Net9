export interface Message {
  id: number
  senderUserName: string
  senderPhotoUrl: string
  senderId: number
  recipientUserName: string
  recipientPhotoUrl: string
  recipientId: number
  content: string
  dateRead?: Date
  messageSent: Date
}
