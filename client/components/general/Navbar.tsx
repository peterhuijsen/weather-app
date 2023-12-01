import UserIcon from "@/components/icons/UserIcon";
import SignOutIcon from "@/components/icons/SignOutIcon";
import React from "react";
import {deleteCookie} from "cookies-next";
import {useRouter} from "next/router";
import MenuIcon from "@/components/icons/MenuIcon";
import ProjectIcon from "@/components/icons/ProjectIcon";

type NavbarProps = {
	selectedMenu: "dashboard" | "profile"
}

export default function Navbar({ selectedMenu } : NavbarProps) {
	const router = useRouter();

	async function logout() {
		deleteCookie("u");
		router.reload();
	}

	return (
		<div className={"absolute top-8 right-8"}>
			<div className={"flex flex-col gap-4"}>
				<button onClick={() => router.replace("/")} className={"flex flex-col p-4 aspect-square bg-white rounded-full shadow-lg group"}>
					<ProjectIcon className={selectedMenu === "dashboard" ? "text-primary w-6 h-6" : "text-neutral-900 w-6 h-6 group-hover:text-primary"} />
				</button>
				<button onClick={() => router.replace("/profile")} className={"flex flex-col p-4 aspect-square bg-white rounded-full shadow-lg group"}>
					<UserIcon className={selectedMenu === "profile" ? "text-primary w-6 h-6" : "text-neutral-900 w-6 h-6 group-hover:text-primary"} />
				</button>
				<button onClick={logout} className={"flex flex-col p-4 aspect-square bg-white rounded-full shadow-lg group"}>
					<SignOutIcon className={"text-neutral-900 w-6 h-6 group-hover:text-primary"} />
				</button>
			</div>
		</div>
	)
}