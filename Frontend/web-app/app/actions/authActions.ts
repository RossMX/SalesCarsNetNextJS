import { auth } from "@/auth";


export async function getCurrentUser() {
    try {
     const session = await auth();
     if (session?.user) {
        return session.user;
     }   
    } catch (error) {
        console.log(error);
    }
    return null;
}