import React, {ChangeEventHandler} from "react";

type ToggleProps = {
    children: React.ReactNode,
}

const Toggle = ({ children } : ToggleProps) => {
    return (
        <div className={"flex flex-row items-center gap-4 text-neutral-900 text-md"}>
            {children}
        </div>
    )
}

type ToggleOptionProps = {
    id: string,
    onChange?: ChangeEventHandler<HTMLInputElement>,
    value?: boolean,
    disabled?: boolean
}

const ToggleOption = ({ id, onChange, value = false, disabled = false }: ToggleOptionProps) => {
    return (
        <div className={"relative inline-flex items-center cursor-pointer"}>
            <input
                className={"absolute w-full h-full peer appearance-none focus:outline-none"}
                onChange={onChange}
                defaultChecked={value}
                disabled={disabled}
                type={"checkbox"}
                id={id}
            />
            <div className={`flex items-center justify-begin p-0.5 peer w-12 h-6 rounded-3xl border-2 border-neutral-300 peer-checked:bg-neutral-300 peer-checked:justify-end peer-focus:ring-2 peer-focus:ring-primary ${disabled ? "bg-neutral-200" : ""}`}>
                <div className={`w-4 h-4 rounded-3xl ${disabled ? "bg-neutral-300" : "bg-neutral-500"}`} />
            </div>
        </div>
        
    )
}

Toggle.Option = ToggleOption;

export default Toggle;